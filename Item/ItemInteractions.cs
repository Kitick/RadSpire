namespace Components {
    using System;
    using Services;
    using ItemSystem;
    using Character;

    public static class ItemInteractions {
        public static bool EquipItem<TEntity, TItem>(this TEntity user, TItem item)
        where TEntity : CharacterBase
        where TItem : Item {
            return item.Equip(user);
        }

        public static bool UnequipItem<TEntity, TItem>(this TEntity user, TItem item)
        where TEntity : CharacterBase
        where TItem : Item {
            return item.Unequip(user);
        }

        public static bool UseItem<TEntity, TItem>(this TEntity user, TItem item)
        where TEntity : CharacterBase
        where TItem : Item {
            return item.Use(user);
        }

        public static bool UseItemOnTarget<TEntity, TItem, TTarget>(this TEntity user, TItem item, TTarget target)
        where TEntity : CharacterBase
        where TItem : Item
        where TTarget : CharacterBase {
            return item.UseOnTarget(user, target);
        }
    }
}