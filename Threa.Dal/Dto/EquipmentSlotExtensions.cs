namespace Threa.Dal.Dto;

/// <summary>
/// Extension methods for EquipmentSlot enum.
/// </summary>
public static class EquipmentSlotExtensions
{
    /// <summary>
    /// Determines if this slot is an implant slot (requires surgery to equip/unequip).
    /// </summary>
    /// <param name="slot">The equipment slot to check.</param>
    /// <returns>True if this is an implant slot.</returns>
    public static bool IsImplant(this EquipmentSlot slot)
    {
        return (int)slot >= 100 && (int)slot < 200;
    }

    /// <summary>
    /// Determines if this slot is a finger/ring slot.
    /// </summary>
    /// <param name="slot">The equipment slot to check.</param>
    /// <returns>True if this is a finger slot.</returns>
    public static bool IsFingerSlot(this EquipmentSlot slot)
    {
        return (int)slot >= 30 && (int)slot <= 44;
    }

    /// <summary>
    /// Determines if this slot is a weapon slot (MainHand, OffHand, TwoHand).
    /// </summary>
    /// <param name="slot">The equipment slot to check.</param>
    /// <returns>True if this is a weapon slot.</returns>
    public static bool IsWeaponSlot(this EquipmentSlot slot)
    {
        return slot == EquipmentSlot.MainHand 
            || slot == EquipmentSlot.OffHand 
            || slot == EquipmentSlot.TwoHand;
    }

    /// <summary>
    /// Determines if this slot is a body armor slot.
    /// </summary>
    /// <param name="slot">The equipment slot to check.</param>
    /// <returns>True if this is a body slot.</returns>
    public static bool IsBodySlot(this EquipmentSlot slot)
    {
        return (int)slot >= 1 && (int)slot <= 19;
    }

    /// <summary>
    /// Gets the display name for an equipment slot.
    /// </summary>
    /// <param name="slot">The equipment slot.</param>
    /// <returns>A human-readable name for the slot.</returns>
    public static string GetDisplayName(this EquipmentSlot slot)
    {
        return slot switch
        {
            EquipmentSlot.None => "Not Equipped",
            EquipmentSlot.Head => "Head",
            EquipmentSlot.Face => "Face",
            EquipmentSlot.Ears => "Ears",
            EquipmentSlot.Neck => "Neck",
            EquipmentSlot.Shoulders => "Shoulders",
            EquipmentSlot.Back => "Back",
            EquipmentSlot.Chest => "Chest",
            EquipmentSlot.ArmLeft => "Left Arm",
            EquipmentSlot.ArmRight => "Right Arm",
            EquipmentSlot.WristLeft => "Left Wrist",
            EquipmentSlot.WristRight => "Right Wrist",
            EquipmentSlot.HandLeft => "Left Hand",
            EquipmentSlot.HandRight => "Right Hand",
            EquipmentSlot.Waist => "Waist",
            EquipmentSlot.Legs => "Legs",
            EquipmentSlot.AnkleLeft => "Left Ankle",
            EquipmentSlot.AnkleRight => "Right Ankle",
            EquipmentSlot.FootLeft => "Left Foot",
            EquipmentSlot.FootRight => "Right Foot",
            EquipmentSlot.MainHand => "Main Hand",
            EquipmentSlot.OffHand => "Off Hand",
            EquipmentSlot.TwoHand => "Two-Handed",
            EquipmentSlot.FingerLeft1 => "Left Thumb",
            EquipmentSlot.FingerLeft2 => "Left Index Finger",
            EquipmentSlot.FingerLeft3 => "Left Middle Finger",
            EquipmentSlot.FingerLeft4 => "Left Ring Finger",
            EquipmentSlot.FingerLeft5 => "Left Pinky",
            EquipmentSlot.FingerRight1 => "Right Thumb",
            EquipmentSlot.FingerRight2 => "Right Index Finger",
            EquipmentSlot.FingerRight3 => "Right Middle Finger",
            EquipmentSlot.FingerRight4 => "Right Ring Finger",
            EquipmentSlot.FingerRight5 => "Right Pinky",
            EquipmentSlot.ImplantNeural => "Neural Implant",
            EquipmentSlot.ImplantOpticLeft => "Left Eye Implant",
            EquipmentSlot.ImplantOpticRight => "Right Eye Implant",
            EquipmentSlot.ImplantAuralLeft => "Left Ear Implant",
            EquipmentSlot.ImplantAuralRight => "Right Ear Implant",
            EquipmentSlot.ImplantCardiac => "Cardiac Implant",
            EquipmentSlot.ImplantSpine => "Spinal Implant",
            EquipmentSlot.ImplantArmLeft => "Left Arm Cybernetic",
            EquipmentSlot.ImplantArmRight => "Right Arm Cybernetic",
            EquipmentSlot.ImplantLegLeft => "Left Leg Cybernetic",
            EquipmentSlot.ImplantLegRight => "Right Leg Cybernetic",
            EquipmentSlot.ImplantSubdermal => "Subdermal Implant",
            EquipmentSlot.ImplantOrgan => "Organ Implant",
            EquipmentSlot.ImplantHandLeft => "Left Hand Cybernetic",
            EquipmentSlot.ImplantHandRight => "Right Hand Cybernetic",
            _ => slot.ToString()
        };
    }
}
