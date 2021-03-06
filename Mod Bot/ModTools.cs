using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ModLibrary
{
    namespace ModTools
    {
        public static class Upgrade
        {
            /// <summary>
            /// Checks whether or not the given upgrade id is already in use
            /// </summary>
            /// <param name="ID">The ID of the upgrade</param>
            /// <returns></returns>
            public static bool IsIDAlreadyUsed(int ID)
            {
                List<UpgradeDescription> upgrades = UpgradeManager.Instance.UpgradeDescriptions;

                for (int i = 0; i < upgrades.Count; i++)
                {
                    if ((int)upgrades[i].UpgradeType == ID)
                        return true;
                }

                return false;
            }
            /// <summary>
            /// Checks whether or not the given upgrade id is already in use
            /// </summary>
            /// <param name="ID">The ID of the upgrade</param>
            /// <returns></returns>
            public static bool IsIDAlreadyUsed(UpgradeType ID)
            {
                return IsIDAlreadyUsed((int)ID);
            }
            /// <summary>
            /// Gets the icon of the specified upgrade
            /// </summary>
            /// <param name="type">The upgrade's corresponding UpgradeType</param>
            /// <param name="level">The level of the upgrade</param>
            /// <returns></returns>
            public static Sprite GetUpgradeIcon(UpgradeType type, int level = 1)
            {
                return UpgradeManager.Instance.GetUpgrade(type, level).Icon;
            }
            /// <summary>
            /// Gets the icon of the specified upgrade
            /// </summary>
            /// <param name="upgrade"></param>
            /// <returns></returns>
            public static Sprite GetUpgradeIcon(UpgradeTypeAndLevel upgrade)
            {
                return GetUpgradeIcon(upgrade.UpgradeType, upgrade.Level);
            }
            /// <summary>
            /// Gets an UpgradeTypeAndLevel from an UpgradeDescription
            /// </summary>
            /// <param name="upgrade"></param>
            /// <returns></returns>
            public static UpgradeTypeAndLevel GetUpgradeTypeAndLevelFromUpgradeDescription(UpgradeDescription upgrade)
            {
                return new UpgradeTypeAndLevel { Level = upgrade.Level, UpgradeType = upgrade.UpgradeType };
            }
            /// <summary>
            /// Gets an UpgradeDescription from an UpgradeType and a level
            /// </summary>
            /// <param name="type"></param>
            /// <param name="level"></param>
            /// <returns></returns>
            public static UpgradeDescription GetUpgradeDescriptionFromTypeAndLevel(UpgradeType type, int level = 1)
            {
                return UpgradeManager.Instance.GetUpgrade(type, level);
            }
            /// <summary>
            /// Gets an UpgradeDescription from an UpgradeTypeAndLevel
            /// </summary>
            /// <param name="upgrade"></param>
            /// <returns></returns>
            public static UpgradeDescription GetUpgradeDescriptionFromTypeAndLevel(UpgradeTypeAndLevel upgrade)
            {
                return GetUpgradeDescriptionFromTypeAndLevel(upgrade.UpgradeType, upgrade.Level);
            }
            /// <summary>
            /// Gives the specified upgrade to a FirstPersonMover
            /// </summary>
            /// <param name="Target"></param>
            /// <param name="Upgrade"></param>
            public static void Give(FirstPersonMover Target, UpgradeDescription Upgrade)
            {
                if (Target == null)
                    return;

                if (Target.IsMainPlayer())
                {
                    GameDataManager.Instance.SetUpgradeLevel(Upgrade.UpgradeType, Upgrade.Level);
                    UpgradeDescription upgrade = UpgradeManager.Instance.GetUpgrade(Upgrade.UpgradeType, Upgrade.Level);
                    GlobalEventManager.Instance.Dispatch("UpgradeCompleted", upgrade);
                }
                else
                {
                    UpgradeCollection upgradeCollection = Target.gameObject.GetComponent<UpgradeCollection>();

                    if (upgradeCollection == null)
                    {
                        debug.Log("Failed to give upgrade '" + Upgrade.UpgradeName + "' (Level: " + Upgrade.Level + ") to " + Target.CharacterName + " (UpgradeCollection is null)", Color.red);
                        return;
                    }

                    UpgradeTypeAndLevel upgradeToGive = new UpgradeTypeAndLevel { UpgradeType = Upgrade.UpgradeType, Level = Upgrade.Level };

                    List<UpgradeTypeAndLevel> upgrades = ((PreconfiguredUpgradeCollection)upgradeCollection).Upgrades.ToList();

                    upgrades.Add(upgradeToGive);

                    ((PreconfiguredUpgradeCollection)upgradeCollection).Upgrades = upgrades.ToArray();
                    ((PreconfiguredUpgradeCollection)upgradeCollection).InitializeUpgrades();

                    Target.RefreshUpgrades();
                }

                Target.SetUpgradesNeedsRefreshing();
            }
            /// <summary>
            /// Gives the specified upgrades to a FirstPersonMover
            /// </summary>
            /// <param name="Target"></param>
            /// <param name="Upgrades"></param>
            public static void Give(FirstPersonMover Target, UpgradeDescription[] Upgrades)
            {
                for (int i = 0; i < Upgrades.Length; i++)
                {
                    Give(Target, Upgrades[i]);
                }
            }
            /// <summary>
            /// Gives the specified upgrade to a FirstPersonMover
            /// </summary>
            /// <param name="Target"></param>
            /// <param name="Upgrade"></param>
            /// <param name="Level"></param>
            public static void Give(FirstPersonMover Target, UpgradeType Upgrade, int Level)
            {
                Give(Target, GetUpgradeDescriptionFromTypeAndLevel(Upgrade, Level));
            }
            /// <summary>
            /// Gives the specified upgrades to a FirstPersonMover
            /// </summary>
            /// <param name="Target"></param>
            /// <param name="Upgrades"></param>
            /// <param name="Levels"></param>
            public static void Give(FirstPersonMover Target, Dictionary<UpgradeType, int> upgrades)
            {
                List<UpgradeType> upgradeTypes = EnumTools.GetValues<UpgradeType>();

                for (int i = 0; i < upgradeTypes.Count; i++)
                {
                    if (upgrades.TryGetValue(upgradeTypes[i], out int level))
                    {
                        Give(Target, GetUpgradeDescriptionFromTypeAndLevel(upgradeTypes[i], level));
                    }
                }
            }
            /// <summary>
            /// Adds a new upgrade to the upgrade tree
            /// </summary>
            /// <param name="UpgradeID">The ID of the upgrade (If ID is already taken, the upgrade will not be added)</param>
            /// <param name="Name"></param>
            /// <param name="Description"></param>
            /// <param name="Icon">The display image of the upgrade</param>
            /// <param name="AngleOffset">The offset angle in the tree</param>
            /// <param name="IsLimited">If the upgrade has a limited number of uses (Like Clone)</param>
            /// <param name="IsRepeatable">Is the upgrade is repeatable (Like Armor)</param>
            /// <param name="MaxUses">Set the max uses of an upgrade (Will do nothing if IsLimited is false)</param>
            /// <param name="SortOrder"></param>
            /// <param name="Requirement">First Requirement</param>
            /// <param name="SecondRequirement">Second Requirement</param>
            /// <returns>The created upgrade, will be null if upgrade with ID already exists</returns>
            public static UpgradeDescription Add(int UpgradeID, string Name, string Description, Sprite Icon, float AngleOffset, bool IsLimited, bool IsRepeatable, int MaxUses, int SortOrder, UpgradeDescription Requirement, UpgradeDescription SecondRequirement)
            {
                if (IsIDAlreadyUsed(UpgradeID))
                    return null;

                UpgradeDescription upgrade = UpgradeManager.Instance.UpgradeDescriptions[0].gameObject.AddComponent<UpgradeDescription>();

                upgrade.AngleOffset = AngleOffset;
                upgrade.CanBeTransferredInMultiplayer = false;
                upgrade.Description = Description;
                upgrade.HideInStoryMode = false;
                upgrade.Icon = Icon;
                upgrade.IsAvailableInMultiplayer = false;
                upgrade.IsConsumable = IsLimited;
                upgrade.IsDisabledInBattleRoyale = true;
                upgrade.IsRepeatable = IsRepeatable;
                upgrade.IsUpgradeVisible = true;
                upgrade.Level = 1;
                upgrade.MaxRepetitions = MaxUses;
                upgrade.RequiredMetagameProgress = MetagameProgress.P0_None;
                upgrade.Requirement = Requirement;
                upgrade.Requirement2 = SecondRequirement;
                upgrade.SkillPointCostBattleRoyale = 5;
                upgrade.SkillPointCostMultiplayer = 100;
                upgrade.SortOrder = SortOrder;
                upgrade.UpgradeName = Name;
                upgrade.UpgradeType = (UpgradeType)UpgradeID;

                UpgradeManager.Instance.UpgradeDescriptions.Add(upgrade);

                return upgrade;
            }
        }

        public static class Clone
        {
            /// <summary>
            /// Spawns a clone in the clone area without the camera panning to it
            /// </summary>
            public static void Spawn()
            {
                CloneManager.Instance.CloneArea.CreateNewClone(false);
                GameDataManager.Instance.IncreaseNumClones(1);
                CloneManager.Instance.CloneArea.PutAllClonesBackToStartingPositions();
            }
            /// <summary>
            /// Spawns clones in the clone area without the camera panning to it
            /// </summary>
            /// <param name="count">The amount to spawn</param>
            public static void Spawn(int count)
            {
                for (int i = 0; i < count; i++)
                {
                    Spawn();
                }

                CloneManager.Instance.CloneArea.PutAllClonesBackToStartingPositions();
            }

            /// <summary>
            /// Removes a clone from the clone area
            /// </summary>
            public static void Remove()
            {
                if (CloneManager.Instance.GetNumClones() == 0)
                    return;
                
                List<MechBodyPart> bodyParts = ((List<FirstPersonMover>)Accessor.GetPrivateField(typeof(CloneArea), "_clones", CloneManager.Instance.CloneArea))[CloneManager.Instance.GetNumClones() - 1].GetAllBodyParts();

                FireSpreadDefinition fireSpread = new FireSpreadDefinition
                {
                    DamageSourceType = DamageSourceType.None,
                    FireType = FireType.BanFire,
                    MinSpreadProbability = 1000f,
                    SpreadProbabilityDropOff = 0f,
                    StartSpreadProbability = 100f,
                    WaitBetweenSpreads = 0.001f
                };

                for (int i = 0; i < bodyParts.Count; i++)
                {
                    bodyParts[i].TryCutVolume(
                        bodyParts[i].transform.position + new Vector3(2f, 0.5f, -2f),
                        bodyParts[i].transform.position + new Vector3(-2f, 0.5f, -2f),
                        bodyParts[i].transform.position + new Vector3(-2f, 0.5f, 2f),
                        bodyParts[i].transform.position + new Vector3(2f, 0.5f, 2f),
                        AttackManager.Instance.GetNextAttackID(), false, null, DamageSourceType.SpeedHackBanFire, fireSpread, false);
                }

                CloneManager.Instance.CloneArea.PutAllClonesBackToStartingPositions();
            }
            /// <summary>
            /// Removes clones from the clone area
            /// </summary>
            /// <param name="count">The amount to remove</param>
            public static void Remove(int count)
            {
                for (int i = 0; i < count; i++)
                {
                    Remove();
                }

                CloneManager.Instance.CloneArea.PutAllClonesBackToStartingPositions();
            }
        }

        public static class Arrow
        {
            /// <summary>
            /// Creates an arrow and makes it fly in the given direction
            /// </summary>
            /// <param name="Owner">The FirstPersonMover that should be considered the owner of the arrow</param>
            /// <param name="StartPosition"></param>
            /// <param name="MoveDir"></param>
            /// <param name="BladeWidth"></param>
            /// <param name="MakeFlyBySound"></param>
            /// <param name="RotationZ"></param>
            /// <returns>The fired arrow</returns>
            public static ArrowProjectile Create(FirstPersonMover Owner, Vector3 StartPosition, Vector3 MoveDir, float BladeWidth = 1f, bool MakeFlyBySound = false, float RotationZ = 0f)
            {
                if (Owner == null)
                    return null;

                ArrowProjectile arrow = ProjectileManager.Instance.CreateInactiveArrow(false);
                arrow.transform.SetParent(LevelSpecificWorldRoot.Instance.transform, true);
                arrow.SetBladeWidth(BladeWidth);
                arrow.StartFlying(StartPosition, MoveDir, MakeFlyBySound, Owner, false, BoltNetwork.serverFrame, RotationZ);

                return arrow;
            }
            
            /// <summary>
            /// Creates an arrow and makes it fly in the given direction
            /// </summary>
            /// <param name="Owner">The FirstPersonMover that should be considered the owner of the arrow</param>
            /// <param name="StartPosition"></param>
            /// <param name="MoveDir"></param>
            /// <param name="fireSpreadDefinition"></param>
            /// <param name="BladeWidth"></param>
            /// <param name="MakeFlyBySound"></param>
            /// <param name="RotationZ"></param>
            /// <returns>The fired arrow</returns>
            public static ArrowProjectile CreateFlaming(FirstPersonMover Owner, Vector3 StartPosition, Vector3 MoveDir, FireSpreadDefinition fireSpreadDefinition, float BladeWidth = 1f, bool MakeFlyBySound = false, float RotationZ = 0f)
            {
                if (Owner == null)
                    return null;

                ArrowProjectile arrow = Create(Owner, StartPosition, MoveDir, BladeWidth, MakeFlyBySound, RotationZ);
                arrow.SetOnFire(fireSpreadDefinition);

                return arrow;
            }
        }

        public static class EnumTools
        {
            /// <summary>
            /// Gets the name of the given value in an enum
            /// </summary>
            /// <typeparam name="T"></typeparam>
            /// <param name="value"></param>
            /// <returns></returns>
            public static string GetName<T>(T value)
            {
                return Enum.GetName(typeof(T), value);
            }
            /// <summary>
            /// Gets all names in the given enum
            /// </summary>
            /// <typeparam name="T"></typeparam>
            /// <returns></returns>
            public static List<string> GetNames<T>()
            {
                return Enum.GetNames(typeof(T)).ToList();
            }
            /// <summary>
            /// Gets all values of an enum
            /// </summary>
            /// <typeparam name="T"></typeparam>
            /// <returns></returns>
            public static List<T> GetValues<T>()
            {
                return ((T[])Enum.GetValues(typeof(T))).ToList();
            }
        }

        public static class CharacterTools
        {
            /// <summary>
            /// Adds a name tag the the specified Character
            /// </summary>
            /// <param name="character"></param>
            /// <param name="DisplayName"></param>
            /// <param name="ColorIsRed">false: Red, true: Green</param>
            /// <param name="OverrideOld"></param>
            public static void AddNameTag(Character character, string DisplayName = "Ally", bool ColorIsRed = false, bool OverrideOld = false)
            {
                if (character.HasNameTag() && !OverrideOld)
                    return;

                Transform nameTag = TwitchEnemySpawnManager.Instance.TwitchEnemyNameTagPool.InstantiateNewObject(false);
                nameTag.SetParent(GameUIRoot.Instance.transform, false);
                nameTag.SetAsFirstSibling();

                EnemyNameTag enemyNameTag = nameTag.GetComponent<EnemyNameTag>();
                EnemyNameTagConfig enemyNameTagConfig = new EnemyNameTagConfig
                {
                    ForceEnemyColor = ColorIsRed,
                    HideAtDistance = CharacterNameTagManager.Instance.HideNameTagsAtDistance,
                    HideIfNotVisible = true
                };

                enemyNameTag.Initialize(character, DisplayName, enemyNameTagConfig);
                character.SetHasNameTag();
            }

            /// <summary>
            /// Gets the MechBodyPart of the given MechBodyPartType (Returns null if the given Character does not have that body type)
            /// </summary>
            /// <param name="character"></param>
            /// <param name="type"></param>
            /// <returns></returns>
            public static MechBodyPart GetBodyPartOfType(Character character, MechBodyPartType type)
            {
                List<MechBodyPart> bodyParts = character.GetAllBodyParts();

                for (int i = 0; i < bodyParts.Count; i++)
                {
                    if (bodyParts[i].PartType == type)
                        return bodyParts[i];
                }

                return null;
            }
            /// <summary>
            /// Gets all MechBodyParts of the given MechBodyPartTypes (Elements in the List will be null if the given Character does not have a MechBodyPartType specified in the List)
            /// </summary>
            /// <param name="character"></param>
            /// <param name="types"></param>
            /// <returns></returns>
            public static List<MechBodyPart> GetBodyPartsOfTypes(Character character, List<MechBodyPartType> types)
            {
                try
                {
                    if (types.Count == 0)
                        return null;
                }
                catch
                {
                    return null;
                }

                List<MechBodyPart> OutputList = new List<MechBodyPart>();
                List<MechBodyPart> bodyParts = new List<MechBodyPart>();

                for (int i = 0; i < bodyParts.Count; i++)
                {
                    if (types.Contains(bodyParts[i].PartType))
                        OutputList.Add(bodyParts[i]);
                }

                return OutputList;
            }

            /// <summary>
            /// Gets a WeaponModel from the given FirstPersonMover from the given WeaponType (Returns null if the FirstPersonMover does not have the weapon)
            /// </summary>
            /// <param name="firstPersonMover"></param>
            /// <param name="type"></param>
            /// <returns></returns>
            public static WeaponModel GetWeaponModelFromType(FirstPersonMover firstPersonMover, WeaponType type)
            {
                if (type == WeaponType.None || firstPersonMover == null)
                    return null;

                WeaponModel[] weaponModels = firstPersonMover.GetCharacterModel().WeaponModels;

                for (int i = 0; i < weaponModels.Length; i++)
                {
                    if (weaponModels[i].WeaponType == type)
                        return weaponModels[i];
                }

                return null;
            }
            /// <summary>
            /// Gets a WeaponModel from the given Character from the given WeaponType (Returns null if the Character does not have the weapon)
            /// </summary>
            /// <param name="character"></param>
            /// <param name="type"></param>
            /// <returns></returns>
            public static WeaponModel GetWeaponModelFromType(Character character, WeaponType type)
            {
                return GetWeaponModelFromType((FirstPersonMover)character, type);
            }

            public static List<Character> GetAllEnemyCharactersInRange(Vector3 origin, float radius)
            {
                List<Character> characters = CharacterTracker.Instance.GetAllLivingCharacters();
                List<Character> charactersInRange = new List<Character>();

                for(int i = 0; i < characters.Count; i++)
                {
                    if (!characters[i].IsPlayerTeam && !characters[i].IsMainPlayer() && Vector3.Distance(origin, characters[i].GetPositionForAIToAimAt()) <= radius)
                        charactersInRange.Add(characters[i]);
                }

                return charactersInRange;
            }
        }

        public static class ListTools
        {
            /// <summary>
            /// Randomly shuffles the given list
            /// </summary>
            /// <typeparam name="T">The type of the list</typeparam>
            /// <param name="ListToRandomize"></param>
            /// <param name="random"></param>
            /// <returns>The shuffled list</returns>
            public static List<T> Shuffle<T>(List<T> ListToRandomize, System.Random random)
            {
                List<T> RandomizedList = new List<T>();

                for (int i = 0; ListToRandomize.Count > 0;)
                {
                    i = random.Next(0, ListToRandomize.Count - 1);

                    RandomizedList.Add(ListToRandomize[i]);
                    ListToRandomize.RemoveAt(i);
                }

                return RandomizedList;
            }
            /// <summary>
            /// Randomly shuffles the given list
            /// </summary>
            /// <typeparam name="T">The type of the list</typeparam>
            /// <param name="ListToRandomize"></param>
            /// <returns>The shuffled list</returns>
            public static List<T> Shuffle<T>(List<T> ListToRandomize)
            {
                return Shuffle(ListToRandomize, new System.Random());
            }
        }

        public static class Vector3Tools
        {
            /// <summary>
            /// Gets a direction from one Vector3 to another
            /// </summary>
            /// <param name="StartPoint"></param>
            /// <param name="Destination"></param>
            /// <returns>The direction between the two points</returns>
            public static Vector3 GetDirection(Vector3 StartPoint, Vector3 Destination)
            {
                return Destination - StartPoint;
            }
        }
    }
}
