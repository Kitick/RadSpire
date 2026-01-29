namespace Components {
    using System;
    using Services;
    using ItemSystem;
    using Character;

    public static class ItemInteractions {
        public static bool EquipItem<TEntity, TItem>(this TEntity user, TItem item)
        where TEntity : CharacterBase
        where TItem : Item {

        }

        public static bool UnequipItem<TEntity, TItem>(this TEntity user, TItem item)
        where TEntity : CharacterBase
        where TItem : Item {

        }

        public static bool UseItem<TEntity, TItem>(this TEntity user, TItem item)
        where TEntity : CharacterBase
        where TItem : Item {

        }

        public static bool UseItemOnTarget<TEntity, TItem, TTarget>(this TEntity user, TItem item, TTarget target)
        where TEntity : CharacterBase
        where TItem : Item
        where TTarget : CharacterBase {

        }
    }
}