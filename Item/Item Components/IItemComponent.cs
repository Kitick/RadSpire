namespace ItemSystem {
	using Godot;

	public interface IItemComponent {

	}

	public interface IUsable {
		public bool CanUse(CharacterBody3D user);
		public bool OnUse(CharacterBody3D user);
	}

	public interface IEquipable {
		public void OnEquip(CharacterBody3D equipper);
		public void OnUnequip(CharacterBody3D unequipper);
	}
}