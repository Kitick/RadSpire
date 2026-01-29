namespace ItemSystem {
	using Godot;

	public interface IItemComponent {

	}

	public interface IItemUseable {
		public bool Use<TEntity>(TEntity user);
	}

	public interface IItemEquipable {
		public bool Equip<TEntity>(TEntity user);
		public bool Unequip<TEntity>(TEntity user);
	}

	public interface IItemUseableOnTarget {
		public bool UseOnTarget<TEntity, TTarget>(TEntity user, TTarget target);
	}
}