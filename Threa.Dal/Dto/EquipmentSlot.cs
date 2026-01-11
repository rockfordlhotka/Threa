namespace Threa.Dal.Dto;

/// <summary>
/// Equipment slots where items can be equipped on a character.
/// </summary>
public enum EquipmentSlot
{
    // Not equipped
    None = 0,

    // Body Slots
    Head = 1,
    Face = 2,
    Ears = 3,
    Neck = 4,
    Shoulders = 5,
    Back = 6,
    Chest = 7,
    ArmLeft = 8,
    ArmRight = 9,
    WristLeft = 10,
    WristRight = 11,
    HandLeft = 12,
    HandRight = 13,
    Waist = 14,
    Legs = 15,
    AnkleLeft = 16,
    AnkleRight = 17,
    FootLeft = 18,
    FootRight = 19,

    // Weapon Slots
    MainHand = 20,
    OffHand = 21,
    TwoHand = 22,

    // Jewelry Slots - Left Hand Fingers
    FingerLeft1 = 30,
    FingerLeft2 = 31,
    FingerLeft3 = 32,
    FingerLeft4 = 33,
    FingerLeft5 = 34,

    // Jewelry Slots - Right Hand Fingers
    FingerRight1 = 40,
    FingerRight2 = 41,
    FingerRight3 = 42,
    FingerRight4 = 43,
    FingerRight5 = 44,

    // Implant Slots (require surgery/procedure to equip/unequip)
    // These represent cybernetic/biotech enhancements
    
    /// <summary>
    /// Neural interface, brain implant.
    /// </summary>
    ImplantNeural = 100,

    /// <summary>
    /// Cybernetic eye replacement or enhancement.
    /// </summary>
    ImplantOpticLeft = 101,

    /// <summary>
    /// Cybernetic eye replacement or enhancement.
    /// </summary>
    ImplantOpticRight = 102,

    /// <summary>
    /// Ear/hearing enhancement implant.
    /// </summary>
    ImplantAuralLeft = 103,

    /// <summary>
    /// Ear/hearing enhancement implant.
    /// </summary>
    ImplantAuralRight = 104,

    /// <summary>
    /// Heart or circulatory system enhancement.
    /// </summary>
    ImplantCardiac = 105,

    /// <summary>
    /// Spinal enhancement or neural booster.
    /// </summary>
    ImplantSpine = 106,

    /// <summary>
    /// Cybernetic arm replacement or enhancement.
    /// </summary>
    ImplantArmLeft = 107,

    /// <summary>
    /// Cybernetic arm replacement or enhancement.
    /// </summary>
    ImplantArmRight = 108,

    /// <summary>
    /// Cybernetic leg replacement or enhancement.
    /// </summary>
    ImplantLegLeft = 109,

    /// <summary>
    /// Cybernetic leg replacement or enhancement.
    /// </summary>
    ImplantLegRight = 110,

    /// <summary>
    /// Subdermal armor, sensors, or other under-skin implants.
    /// </summary>
    ImplantSubdermal = 111,

    /// <summary>
    /// Internal organ replacement or enhancement.
    /// </summary>
    ImplantOrgan = 112,

    /// <summary>
    /// Hand/finger enhancement (separate from arm).
    /// </summary>
    ImplantHandLeft = 113,

    /// <summary>
    /// Hand/finger enhancement (separate from arm).
    /// </summary>
    ImplantHandRight = 114
}

