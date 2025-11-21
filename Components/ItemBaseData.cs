using System;
using SaveSystem;

namespace Components {
    public enum ItemType { Weapon, Armor, Resource, Placeable }

	public class ItemBaseData : ISaveable<ItemBaseDataData> {
        public string Id {
            get;
            set {
                if(string.IsNullOrWhiteSpace(value)) {
                    return;
                }
                field = value;
            }
        }

        public string Name {
            get;
            set {
                if(string.IsNullOrWhiteSpace(value)) {
                    return;
                }
                field = value;
            }
        }

        public string Description {
            get;
            set {
                if(string.IsNullOrWhiteSpace(value)) {
                    return;
                }
                field = value;
            }
        }

        public bool IsStackable {
            get;
            set;
        }
        
        public int MaxStackSize {
            get;
            set;
        }

        public ItemType Type {
            get;
            set;
        }

        public bool IsConsumable {
            get;
            set;
        }

		public ItemBaseDataData Serialize() => new ItemBaseDataData {
            Id = Id,
            Name = Name,
            Description = Description,
            IsStackable = IsStackable,
            MaxStackSize = MaxStackSize,
            Type = Type,
            IsConsumable = IsConsumable
        };

		public void Deserialize(in ItemBaseDataData data) {
            Id = data.Id;
			Name = data.Name;
            Description = data.Description;
            IsStackable = data.IsStackable;
            MaxStackSize = data.MaxStackSize;
            Type = data.Type;
            IsConsumable = data.IsConsumable;
		}
	}
}

namespace SaveSystem {
	public readonly struct ItemBaseDataData : ISaveData {
        public string Id { get; init; }
        public string Name { get; init; }
        public string Description { get; init; }
        public bool IsStackable { get; init; }
        public int MaxStackSize { get; init; }
        public Components.ItemType Type { get; init; }
        public bool IsConsumable { get; init; }
	}
}