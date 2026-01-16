using System.Collections.Generic;
using Threa.Dal.Dto;

namespace GameMechanics.Combat
{
  /// <summary>
  /// Maps equipment slots to hit locations for armor coverage determination.
  /// </summary>
  public static class EquipmentLocationMapper
  {
    /// <summary>
    /// Gets the hit locations covered by armor equipped in a specific slot.
    /// </summary>
    /// <param name="slot">The equipment slot.</param>
    /// <returns>Array of hit locations covered by armor in this slot.</returns>
    public static HitLocation[] GetCoveredLocations(EquipmentSlot slot)
    {
      return slot switch
      {
        // Head coverage
        EquipmentSlot.Head => [HitLocation.Head],
        EquipmentSlot.Face => [HitLocation.Head],

        // Torso coverage
        EquipmentSlot.Chest => [HitLocation.Torso],
        EquipmentSlot.Back => [HitLocation.Torso],
        EquipmentSlot.Shoulders => [HitLocation.Torso],
        EquipmentSlot.Waist => [HitLocation.Torso],

        // Arm coverage
        EquipmentSlot.ArmLeft => [HitLocation.LeftArm],
        EquipmentSlot.ArmRight => [HitLocation.RightArm],
        EquipmentSlot.WristLeft => [HitLocation.LeftArm],
        EquipmentSlot.WristRight => [HitLocation.RightArm],
        EquipmentSlot.HandLeft => [HitLocation.LeftArm],
        EquipmentSlot.HandRight => [HitLocation.RightArm],

        // Leg coverage
        EquipmentSlot.Legs => [HitLocation.LeftLeg, HitLocation.RightLeg],
        EquipmentSlot.AnkleLeft => [HitLocation.LeftLeg],
        EquipmentSlot.AnkleRight => [HitLocation.RightLeg],
        EquipmentSlot.FootLeft => [HitLocation.LeftLeg],
        EquipmentSlot.FootRight => [HitLocation.RightLeg],

        // Implants (subdermal covers torso)
        EquipmentSlot.ImplantSubdermal => [HitLocation.Torso],
        EquipmentSlot.ImplantArmLeft => [HitLocation.LeftArm],
        EquipmentSlot.ImplantArmRight => [HitLocation.RightArm],
        EquipmentSlot.ImplantLegLeft => [HitLocation.LeftLeg],
        EquipmentSlot.ImplantLegRight => [HitLocation.RightLeg],

        // Weapons, jewelry, and other slots don't provide armor coverage
        _ => []
      };
    }

    /// <summary>
    /// Gets all equipment slots that could provide armor for a specific hit location.
    /// </summary>
    /// <param name="location">The hit location.</param>
    /// <returns>Array of equipment slots that cover this location.</returns>
    public static EquipmentSlot[] GetSlotsForLocation(HitLocation location)
    {
      return location switch
      {
        HitLocation.Head => [EquipmentSlot.Head, EquipmentSlot.Face],
        HitLocation.Torso => [EquipmentSlot.Chest, EquipmentSlot.Back, EquipmentSlot.Shoulders,
                              EquipmentSlot.Waist, EquipmentSlot.ImplantSubdermal],
        HitLocation.LeftArm => [EquipmentSlot.ArmLeft, EquipmentSlot.WristLeft,
                                EquipmentSlot.HandLeft, EquipmentSlot.ImplantArmLeft],
        HitLocation.RightArm => [EquipmentSlot.ArmRight, EquipmentSlot.WristRight,
                                 EquipmentSlot.HandRight, EquipmentSlot.ImplantArmRight],
        HitLocation.LeftLeg => [EquipmentSlot.Legs, EquipmentSlot.AnkleLeft,
                                EquipmentSlot.FootLeft, EquipmentSlot.ImplantLegLeft],
        HitLocation.RightLeg => [EquipmentSlot.Legs, EquipmentSlot.AnkleRight,
                                 EquipmentSlot.FootRight, EquipmentSlot.ImplantLegRight],
        _ => []
      };
    }

    /// <summary>
    /// Checks if an equipment slot provides armor coverage.
    /// </summary>
    /// <param name="slot">The equipment slot.</param>
    /// <returns>True if armor in this slot provides hit protection.</returns>
    public static bool IsArmorSlot(EquipmentSlot slot)
    {
      return GetCoveredLocations(slot).Length > 0;
    }
  }
}
