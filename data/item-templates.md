items.csv/tsv (Basic Items)
  Required: Name (unique)
  Optional: Description, ShortDescription, ItemType, Weight (>=0), Volume, Value (>=0), Rarity, IsStackable,
MaxStackSize, EquipmentSlot
  ItemType values: Miscellaneous, Consumable, Container, Jewelry, Clothing, Tool, Component, QuestItem, Currency
  Rarity values: Common, Uncommon, Rare, Epic, Legendary, Artifact
  EquipmentSlot values: None, Head, Face, Neck, Shoulders, Chest, Arms, Hands, Waist, Legs, Feet, MainHand, OffHand,
Ring, Back

weapons.csv/tsv (Melee Weapons)
  Required: Name (unique), Skill, DamageClass (1-6), DamageType
  Optional: Description, ShortDescription, Weight (>=0), Volume, Value (>=0), Rarity, WeaponType, SVModifier,
AVModifier, EquipmentSlot, MinSkillLevel
  DamageType values: Bashing, Cutting, Piercing, Projectile, Energy
  WeaponType values: None, Sword, Axe, Mace, Spear, Dagger, Staff, Unarmed, Bow, Crossbow, Thrown, Pistol, Rifle,
Shotgun, SMG, Wand

ranged-weapons.csv/tsv (Ranged Weapons)
  Required: Name (unique), Skill, DamageClass (1-6), ShortRange (>0), MediumRange (>Short), LongRange (>Medium),
ExtremeRange (>Long), Capacity (>0), AmmoType
  Optional: Description, ShortDescription, Weight, Volume, Value, Rarity, DamageType, SVModifier, AVModifier,
RangedWeaponType, ChamberCapacity, ReloadType, AcceptsLooseAmmo, FireModes, BurstSize, SuppressiveRounds, IsDodgeable
  ReloadType values: Magazine, SingleRound, Cylinder, Belt, Battery
  FireModes: Comma-separated list of: Single, Burst, Suppression, AOE

armor.csv/tsv (Armor & Shields)
  Required: Name (unique), EquipmentSlot
  Optional: Description, ShortDescription, Weight (>=0), Volume, Value, Rarity, Skill, DodgeModifier
  Absorption columns: AbsorbBashing (0-20), AbsorbCutting (0-20), AbsorbPiercing (0-20), AbsorbProjectile (0-20),
AbsorbEnergy (0-20)
  Note: EquipmentSlot=OffHand creates Shield, other slots create Armor

ammo.csv/tsv (Ammunition)
  Required: Name (unique), AmmoType
  Optional: Description, ShortDescription, Weight, Volume, Value, Rarity, MaxStackSize, DamageModifier, SpecialEffect,
DamageType, IsContainer, ContainerCapacity
  Note: If IsContainer=true, ContainerCapacity must be > 0 (for magazines)