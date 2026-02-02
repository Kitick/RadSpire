namespace Components {
    using System;
    using Godot;
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

    public partial class UseItem : Node {
        private static readonly LogService Log = new(nameof(UseItem), enabled: true);
        public CharacterBase? User;
        public Item? ItemToUse;
        private Action? UnsubscribeUse;

        public override void _Ready() {
            SetInputCallbacks();
        }

        void SetInputCallbacks() {
            UnsubscribeUse = ActionEvent.Consume.WhenPressed(() => {
                Log.Info("UseItem action pressed.");
                if(User == null || ItemToUse == null) {
                    Log.Info("User or ItemToUse is null, cannot use item.");
                    return;
                }
                else {
                    Log.Info("Attempting to use item.");
                    bool success = User.UseItem(ItemToUse);
                    if(success) {
                        Log.Info("Item used successfully.");
                    }
                    else {
                        Log.Info("Failed to use item.");
                    }
                }
            });
        }
    }
}