using Godot;

namespace Components {
    public interface IItemComponent {

    }

    public interface IUsable{
        public bool CanUse(CharacterBody3D user);
        public bool OnUse(CharacterBody3D user);
    }

    public interface IConsumable {
        public bool CanConsume(CharacterBody3D consumer);
        public bool OnConsume(CharacterBody3D consumer);
    }

    public interface IEquipable {
        public void OnEquip(CharacterBody3D equipper);
        public void OnUnequip(CharacterBody3D unequipper);
    }
    


}