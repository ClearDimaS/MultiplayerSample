using Codice.CM.Triggers;
using Fusion;
using FusionExamples.Tanknarok;
using MS.Core;
using MS.GameSession;
using MS.Level;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using Zenject;

namespace MS.Player
{

    public interface IConsumable
    {
        public void Consume(Player player);
    }

    [RequireComponent(typeof(NetworkCharacterControllerPrototype))]
    public class Player : NetworkBehaviourDI, ICanTakeDamage, IPlayer
    {
        public const byte MAX_HEALTH = 100;

        public enum State
        {
            New,
            Despawned,
            Spawning,
            Active,
            Dead
        }

        [Inject] private IShooter shooter;
        [Inject] private GameSessionManager gameSessionManager;
        [Inject] private LevelManager levelManager;
        [Inject] private IPlayerManager playerManager;

        [Header("Visuals")]
        [SerializeField] private Material[] playerMaterials;
        [SerializeField] private Transform hull;
        [SerializeField] private Transform turret;
        [SerializeField] private Transform visualParent;

        [Space(10)]
        [SerializeField] private float pickupRadius;
        [SerializeField] private float respawnTime;
        [SerializeField] private LayerMask pickupMask;

        [Networked(OnChanged = nameof(OnStateChanged))]
        public State state { get; set; }
        [Networked]
        public byte coins { get; set; }
        [Networked]
        public byte life { get; set; }

        [Networked]
        public NetworkString<_128> playerName { get; set; }

        [Networked]
        private Vector2 moveDirection { get; set; }

        [Networked]
        private Vector2 aimDirection { get; set; }

        [Networked]
        private TickTimer respawnTimer { get; set; }

        [Networked]
        private TickTimer invulnerabilityTimer { get; set; }

        [Networked]
        public byte lives { get; set; }

        [Networked]
        public byte score { get; set; }

        public static Player local { get; set; }

        public bool isActivated => (gameObject.activeInHierarchy && (state == State.Active || state == State.Spawning));
        public bool isDead => state == State.Dead;
        public bool isRespawningDone => state == State.Spawning && respawnTimer.Expired(Runner);

        public IShooter Shooter => shooter;
        public Material playerMaterial { get; set; }
        public GameObject GameObject => gameObject;
        public Vector3 velocity => cc.Velocity;
        public Quaternion hullRotation => hull.rotation;

        public int PlayerID => playerID;

        public NetworkObject NT_Object => Object;

        private int playerID;
        private NetworkCharacterControllerPrototype cc;
        private Collider[] overlaps = new Collider[1];
        private Collider collider;
        private HitboxRoot hitBoxRoot;
        private Vector2 lastMoveDirection; // Store the previous direction for correct hull rotation
        private float respawnInSeconds = -1;

        protected override void OnAwake()
        {
            base.OnAwake();
            cc = GetComponent<NetworkCharacterControllerPrototype>();
            collider = GetComponentInChildren<Collider>();
            hitBoxRoot = GetComponent<HitboxRoot>();
        }
 
        public void InitNetworkState(byte maxLives)
        {
            state = State.New;
            lives = maxLives;
            life = MAX_HEALTH;
            score = 0;
            coins = 0;
        }

        public override void Spawned()
        {
            if (Object.HasInputAuthority)
                local = this;

            // Getting this here because it will revert to -1 if the player disconnects, but we still want to remember the Id we were assigned for clean-up purposes
            playerID = Object.InputAuthority;

            SetMaterial();

            playerManager.AddPlayer(this);

            // Auto will set proxies to InterpolationDataSources.Snapshots and State/Input authority to InterpolationDataSources.Predicted
            // The NCC must use snapshots on proxies for lag compensated raycasts to work properly against them.
            // The benefit of "Auto" is that it will update automatically if InputAuthority is changed (this is not relevant in this game, but worth keeping in mind)
            GetComponent<NetworkCharacterControllerPrototype>().InterpolationDataSource = InterpolationDataSources.Auto;
        }

        public override void FixedUpdateNetwork()
        {
            if (Object.HasStateAuthority)
            {
                if (respawnInSeconds >= 0)
                    CheckRespawn();

                if (isRespawningDone)
                    ResetPlayer();
            }

            CheckForPowerupPickup();
        }

        /// <summary>
        /// Render is the Fusion equivalent of Unity's Update() and unlike FixedUpdateNetwork which is very different from FixedUpdate,
        /// Render is in fact exactly the same. It even uses the same Time.deltaTime time steps. The purpose of Render is that
        /// it is always called *after* FixedUpdateNetwork - so to be safe you should use Render over Update if you're on a
        /// SimulationBehaviour.
        ///
        /// Here, we use Render to update visual aspects of the Tank that does not involve changing of networked properties.
        /// </summary>
        public override void Render()
        {
            visualParent.gameObject.SetActive(state == State.Active);
            collider.enabled = state != State.Dead;
            hitBoxRoot.HitboxRootActive = state == State.Active;

            // Add a little visual-only movement to the mesh
            SetMeshOrientation();

            if (moveDirection.magnitude > 0.1f)
                lastMoveDirection = moveDirection;
        }

        private void SetMaterial()
        {
            playerMaterial = playerMaterials[playerID % playerMaterials.Length];
            PlayerGFX[] parts = GetComponentsInChildren<PlayerGFX>();
            foreach (var part in parts)
            {
                part.SetMaterial(playerMaterial);
            }
        }

        /// <summary>
        /// Control the rotation of hull and turret
        /// </summary>
        private void SetMeshOrientation()
        {
            if (moveDirection.magnitude > 0.1f)
                hull.forward = Vector3.Lerp(hull.forward, new Vector3(moveDirection.x, 0, moveDirection.y), Time.deltaTime * 10f);

            if (aimDirection.sqrMagnitude > 0)
                turret.forward = Vector3.Lerp(turret.forward, new Vector3(aimDirection.x, 0, aimDirection.y), Time.deltaTime * 100f);
        }

        /// <summary>
        /// Set the direction of movement and aim
        /// </summary>
        public void SetDirections(Vector2 moveDirection, Vector2 aimDirection)
        {
            this.moveDirection = moveDirection;
            this.aimDirection = aimDirection;
        }

        public void Move()
        {
            if (!isActivated)
                return;

            cc.Move(new Vector3(moveDirection.x, 0, moveDirection.y));
        }

        /// <summary>
        /// Apply an impulse to the Tank - in the absence of a rigidbody and rigidbody physics, we're emulating a physical impact by
        /// adding directly to the Tanks controller velocity. I'm sure Newton is doing a few extra turns in his grave over this, but for a
        /// cartoon style game like this, it's all about how it looks and feels, and not so much about being correct :)...
        /// </summary>
        /// <param name="impulse">Size and direction of the impulse</param>
        public void ApplyImpulse(Vector3 impulse)
        {
            if (!isActivated)
                return;

            if (Object.HasStateAuthority)
            {
                cc.Velocity += impulse / 10.0f; // Magic constant to compensate for not properly dealing with masses
                cc.Move(Vector3.zero); // Velocity property is only used by CC when steering, so pretend we are, without actually steering anywhere
            }
        }

        /// <summary>
        /// Apply damage to Tank with an associated impact impulse
        /// </summary>
        /// <param name="impulse"></param>
        /// <param name="damage"></param>
        /// <param name="attacker"></param>
        public void ApplyDamage(Vector3 impulse, byte damage, PlayerRef attacker)
        {
            if (!isActivated || !invulnerabilityTimer.Expired(Runner))
                return;

            //Don't damage yourself
            var attackingPlayer = playerManager.Get(attacker);
            if (attackingPlayer != null && attackingPlayer.PlayerID == playerID)
                return;

            ApplyImpulse(impulse);

            if (damage >= life)
            {
                life = 0;
                state = State.Dead;

                if (gameSessionManager.PlayState == PlayState.MATCH)
                    lives -= 1;

                if (lives > 0)
                    Respawn(respawnTime);

                gameSessionManager.OnPlayerDeath();
            }
            else
            {
                life -= damage;
                Debug.Log($"Player {playerID} took {damage} damage, life = {life}");
            }

            invulnerabilityTimer = TickTimer.CreateFromSeconds(Runner, 0.1f);
        }

        public void AddCoins() 
        {
            coins += 1;
        }

        public void Respawn(float inSeconds)
        {
            respawnInSeconds = inSeconds;
        }

        private void CheckRespawn()
        {
            if (respawnInSeconds > 0)
                respawnInSeconds -= Runner.DeltaTime;
            SpawnPoint spawnpt = levelManager.GetPlayerSpawnPoint(playerID);
            if (spawnpt != null && respawnInSeconds <= 0)
            {
                Debug.Log($"Respawning player {playerID}, life={life}, lives={lives}, hasAuthority={Object.HasStateAuthority} from state={state}");

                // Make sure we don't get in here again, even if we hit exactly zero
                respawnInSeconds = -1;

                // Restore health
                life = MAX_HEALTH;
                coins = 0;

                // Start the respawn timer and trigger the teleport in effect
                respawnTimer = TickTimer.CreateFromSeconds(Runner, 1);
                invulnerabilityTimer = TickTimer.CreateFromSeconds(Runner, 1);

                // Place the tank at its spawn point. This has to be done in FUN() because the transform gets reset otherwise
                Transform spawn = spawnpt.transform;
                transform.position = spawn.position;
                transform.rotation = spawn.rotation;

                // If the player was already here when we joined, it might already be active, in which case we don't want to trigger any spawn FX, so just leave it ACTIVE
                if (state != State.Active)
                    state = State.Spawning;

                Debug.Log($"Respawned player {playerID}, tick={Runner.Simulation.Tick}, timer={respawnTimer.IsRunning}:{respawnTimer.TargetTick}, life={life}, lives={lives}, hasAuthority={Object.HasStateAuthority} to state={state}");
            }
        }

        public static void OnStateChanged(Changed<Player> changed)
        {
            if (changed.Behaviour)
                changed.Behaviour.OnStateChanged();
        }

        public void OnStateChanged()
        {
            if(state == State.Dead)
                visualParent.gameObject.SetActive(false);
        }

        private void ResetPlayer()
        {
            Debug.Log($"Resetting player {playerID}, tick={Runner.Simulation.Tick}, timer={respawnTimer.IsRunning}:{respawnTimer.TargetTick}, life={life}, lives={lives}, hasAuthority={Object.HasStateAuthority} to state={state}");
            state = State.Active;
        }

        public override void Despawned(NetworkRunner runner, bool hasState)
        {
            playerManager.RemovePlayer(this);
        }

        public void DespawnTank()
        {
            if (state == State.Dead)
                return;

            state = State.Despawned;
        }

        public async void TriggerDespawn()
        {
            DespawnTank();
            playerManager.RemovePlayer(this);

            await Task.Delay(300); // wait for effects

            if (Object == null) { return; }

            if (Object.HasStateAuthority)
            {
                Runner.Despawn(Object);
            }
            else if (Runner.IsSharedModeMasterClient)
            {
                Object.RequestStateAuthority();

                while (Object.HasStateAuthority == false)
                {
                    await Task.Delay(100); // wait for Auth transfer
                }

                if (Object.HasStateAuthority)
                {
                    Runner.Despawn(Object);
                }
            }
        }

        public void InitNetworkState(int maxLives)
        {
            lives = (byte)maxLives;
        }

        /// <summary>
        /// Called when a player collides with a powerup.
        /// </summary>


        private void CheckForPowerupPickup()
        {
            // If we run into a powerup, pick it up
            if (isActivated && Runner.GetPhysicsScene().OverlapSphere(transform.position, pickupRadius, overlaps, pickupMask, QueryTriggerInteraction.Collide) > 0)
            {
                var consumable = overlaps[0].GetComponent<IConsumable>();
                Pickup(consumable);
            }
        }

        private void Pickup(IConsumable consumable)
        {
            if (consumable == null)
                return;

            consumable.Consume(this);
        }
    }
}
