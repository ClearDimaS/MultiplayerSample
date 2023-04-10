using UnityEngine;

namespace FusionExamples.Tanknarok
{
	public enum PowerupType
	{
		DEFAULT = 0,
		COIN = 1,
		EMPTY = 2
	}
	
	[CreateAssetMenu(fileName = "PE_", menuName = "ScriptableObjects/PowerupElement")]
	public class PowerupElement : ScriptableObject
	{
		public PowerupType powerupType;
		public Mesh powerupSpawnerMesh;
	}
}