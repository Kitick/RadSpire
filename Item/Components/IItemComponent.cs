namespace ItemSystem;

public interface IItemComponent {
	public int priority { get; init; }
	public string[] getComponentDescription();
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
