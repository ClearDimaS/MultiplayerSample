using Fusion;
using MS.Core;
using MS.Player;
using UnityEngine;
using Random = UnityEngine.Random;

namespace FusionExamples.Tanknarok
{
	/// <summary>
	/// Powerups are spawned by the LevelManager and, when picked up, changes the
	/// current weapon of the tank.
	/// </summary>
	public class PowerupSpawner : NetworkBehaviourDI, IConsumable
	{
		[SerializeField] private PowerupElement[] _powerupElements;
		[SerializeField] private Renderer _renderer;
		[SerializeField] private MeshFilter _meshFilter;

		[Header("Colors")] 
		[SerializeField] private Color _mainPowerupColor;
		[SerializeField] private Color _specialPowerupColor;
		[SerializeField] private Color _buffPowerupColor;

		[Networked(OnChanged = nameof(OnRespawningChanged))]
		public NetworkBool isRespawning { get; set; }

		[Networked]
		public int activePowerupIndex { get; set; }

		[Networked]
		public float respawnTimerFloat { get; set; }

		private float _respawnDuration = 3f;
		public float respawnProgress => respawnTimerFloat / _respawnDuration;

		void OnEnable()
		{

		}
		
		public override void Spawned()
		{
			_renderer.enabled = false;
			isRespawning = true;
			SetNextPowerup();
		}

		public override void FixedUpdateNetwork()
		{
			if (!Object.HasStateAuthority)
				return;

			// Update the respawn timer
			respawnTimerFloat = Mathf.Min(respawnTimerFloat + Runner.DeltaTime, _respawnDuration);

			// Spawn a new powerup whenever the respawn duration has been reached
			if (respawnTimerFloat >= _respawnDuration && isRespawning)
			{
				isRespawning = false;
			}
		}

		// Create a simple scale in effect when spawning
		public override void Render()
		{
			if (!isRespawning)
			{
				_renderer.transform.localScale = Vector3.Lerp(_renderer.transform.localScale, Vector3.one, Time.deltaTime * 5f);
			}
			else
			{
				_renderer.transform.localScale = Vector3.zero;
			}
		}

		/// <summary>
		/// Get the pickup contained in this spawner and trigger the spawning of a new powerup
		/// </summary>
		/// <returns></returns>
		public PowerupElement Pickup()
		{
			if (isRespawning)
				return null;

			// Store the active powerup index for returning
			int lastIndex = activePowerupIndex;

			// Trigger the pickup effect, hide the powerup and select the next powerup to spawn
			if (respawnTimerFloat >= _respawnDuration)
			{
				if (_renderer.enabled)
				{
					_renderer.enabled = false;
					SetNextPowerup();
				}
			}
			return lastIndex != -1 ? _powerupElements[lastIndex] : null;
		}

		private void SetNextPowerup()
		{
			if (Object.HasStateAuthority)
			{
				activePowerupIndex = Random.Range(0, _powerupElements.Length);
				respawnTimerFloat = 0;
				isRespawning = true;
			}
		}

        public void Consume(Player player)
        {
            if (Pickup() != null)
            {
                ConsumeInternal(player);
            }
        }

        public static void OnRespawningChanged(Changed<PowerupSpawner> changed)
		{
			if(changed.Behaviour)
				changed.Behaviour.OnRespawningChanged();
		}

		private void OnRespawningChanged()
		{
			_renderer.enabled = true;
			_meshFilter.mesh = _powerupElements[activePowerupIndex].powerupSpawnerMesh;
		}

        private void ConsumeInternal(Player player)
        {
            if (_powerupElements[activePowerupIndex].powerupType == PowerupType.COIN)
                player.AddCoins();
        }
    }
}