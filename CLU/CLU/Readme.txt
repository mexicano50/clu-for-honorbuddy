#region Revision info
/*
 * $Author$
 * $Date$
 * $ID$
 * $Revision$
 * $URL$
 * $LastChangedBy$
 * $ChangesMade$
 */
#endregion


CLU (Codified Likeness Utility) the Rotation CC for Combat/Raid/Lazyraider bots Powered by Felmaster technology

Current Issues Updated: 20/September/2012
=================================
None. - well maybe a few :P

Description
==========
This CC is mainly intended for PVE users of honorbuddy, however it can still be used for PVP it will simply use the PVE Rotation at the moment. 
(Class Specific PVP rotations may be added in the future).It will perform its rotation according to online resources such as Elitist Jerks 
& SimulationCraft, leaving you to monitor more important things such as fight mechanics.

Developer's Note
============
This CC was formed as a hobby.
Donations are not permitted.
Support is provided, however many of us have jobs and families so do not expect 24hr support.
Constructive criticism is welcome.

Installation
============

*First Step - Important Do not skip this step!*
--------------
*Update to Microsoft .NET Framework 4 http://www.microsoft.com/en-us/download/details.aspx?id=17851
*Set your WoW Client to 32bit


Method 1: Zip File !! *Note: Currently not available until the official Honorbuddy release. !!
-------------------

* You can download a stable version from the HB forum post - Download the latest zip file.
* Copy the CLU folder into "%Your Honorbuddy Folder%\Routines\"
* Load Honorbuddy
* Select "Combat" as your bot base (Lazyraider and RaidBot tested OK)
* Select "Load Profile" and load the BlankProfile.xml (Provided at the end of this post.)
* Start Honorbuddy.
* Stay out of the fire and point to kill.

Method 2: SVN
------------------

* You can access the latest CLU build from the following SVN Link http://clu-for-honorbuddy.googlecode.com/svn/trunk/CLU/
* Google: How to use TortoiseSVN. http://tortoisesvn.net
* Copy the CLU folder into "%Your Honorbuddy Folder%\Routines\"
* Load Honorbuddy
* Select "Combat" as your bot base (Lazyraider and RaidBot are supported)
* Select "Load Profile" and load the BlankProfile.xml (Provided at the end of this post.)
* Start Honorbuddy.
* Stay out of the fire and point to kill.


Features
========

Current Supported Rotations - All rotations capable of combat. The [U]untested rotations are from patch 4.3
------------------------------

Three Rotation Modes - Single, PVE, PVP
- Key: Tested[T], Untested[U], Partially checked[PC], NotImplemented[N/A], , Checked/Some Abilities Missing[x]

* DeathKnight
- [T] Blood
- [T] Frost
- [T] Unholy

* Druid
- [T] Balance
- [PC] Feral
- [PC] Guardian	
- [U] Restoration

* Hunter
- [T] BeastMastery
- [T] Marksmanship
- [T] Survival

* Mage
- [T] Arcane
- [T] Fire
- [U] Frost

* Monk
- [U] Brewmaster
- [U] Mistweaver
- [U] Windwalker

* Paladin
- [T] Holy
- [T] Protection
- [T] Retribution

* Priest
- [T] Shadow
- [U] Discipline
- [U] Holy	

* Rogue
- [T] Assassination
- [T] Combat
- [U] Subtlety	

* Shaman
- [U] Elemental	 	
- [T] Restoration
- [U] Enhancement

* Warlock
- [T] Affliction
- [U] Demonology
- [T] Destruction

* Warrior
- [T] Arms
- [T] Protection
- [T] Fury
	
Rotation Selector
--------------------

Clu has the special abilitie to load Custom Rotations. This means you can copy an existing rotation and modify it to your needs and CLU 
will load it for you to use.

1) goto "%Your Honorbuddy Folder%\CustomClasses\Classes\" folder and select the class folder you want to customize.
2) copy and paste the rotation file you want to change.
3) rename the file to a different name (i.e. Elemental.cs to Elemental_v2.cs)
4) open the file you just renamed and look for the line that says "class Elemental : RotationBase" and change it to "class Elemental_v2 : RotationBase".
5) in the same file look for the line that says "public override string Name { get { return "Elemental Shaman"; } }" and rename it to public override string Name { get { return "Elemental Shaman v2"; } }
6) Make any necessary changes to the rotation you want.
7) Congratulations you just made a custom rotation file of your own. When you next run CLU it will prompt you to select a rotation.

Rotation Overide
-------------------

Under General Settings in the GUI you can overide the Rotation in use, meaning if you are standing in a capital city you can overide the SOLO rotation and use the PvP rotation.
1) General > Do not Touch > Enable Rotation > TRUE
2) General > Do not Touch > Rotation Overide > Solo,Battleground,PVE
2) Click > "SwapRotation"

Do not fucking touch if you do not know wtf you are doing...or -50dkp for you!
 

Utility
-------------

* Trinkets
- Will Use trinkets on cooldown if the target is a boss or a PvP player
- All level 85-90 Trinkets with a "USE" affect supported.

* Racials
- Will Use *most* Racials on cooldown if the target is a boss or a PvP player 

* Totems
- Handles Earth, Water, Air totems (Fire totems are specified within the specific rotation)

* Poisons
- Handles the application of poisons for rogues.


Miscellaneous functionality
-----------------------------------------------

* Multi-Dotting
- When enabled it will cycle threw all targets and apply appropriate DoT's

* Cooldown Management
- Intelligent use of cooldowns by confirming if your target is worthy.
- Combat resurrection (Resto Druid only atm)
- Engineering gloves
- Boss, PvP Player, or Mobs (for questing)

* Extra Action button Click
- While in DeathWing (Cataclysm) CLU will click the Extra Action Button during the encounter for you.

* Interrupts
- It will not try to interrupt non-interruptible abilities

* Buffing
- Apply a buff or debuff depending on other players providing that buff or debuff.

* AoE
- Clustered pack detection (for AoE)
- Intelligent AoE - checking for controlled mobs.

* Tier piece detection
- Detection of how many Tier pieces a player is wearing to dynamically change rotation priorities (Only built with feral druid and retribution paladin and Shadow Priest)

*Talents
- CLU is capable of detecting what specialization you are and setting an appropriate rotation

* Glyphs
- CLU is capable of detecting Glyphs you have and will change its rotation to accommodate them

* Bag Items Support
- Using a bag item (Healthstone, Flask, Potion)

* Resting/ Self Healing
- Emergency healing via class specific spells/abilities.
- Will Drink and Eat food

Managers
------------------

* Pet Manager
- Handles the calling of pets, pet spells, pet coooldowns

* SpellImunity Manager
- Handles tracking of target immunities to Nature, Fire, Frost, etc

* Bot Management
- Handles checking what the current botbase is and how CLU should set its configuration

* CombatLogEvents Manager
- Handles events that happen within the WoW combat Log, Spell Missed, Target Evading, etc

* EncounterSpecific
- Handles specific functions for encounters 
	
	
Movement
------------------

CLU will take over movement when the botbase (questing, bgbuddy,gatherbuddy,etc) gives control to CLU.

 * Overview:
- Acquire Target
- Face Target
- Move to Location (LoS)
- Blacklist evading targets
- Move behind Target (If enabled in the UI Settings)
- Move towards target if it exceeds minimum/maximum combat distance
- Stop moving if it meets minimum/maximum combat distance
- Considers both melee and ranged characters (i.e. melee will move within melee range, and ranged will stay at maximum range.)

Targeting
------------------

CLU will take over targeting when the botbase (questing, bgbuddy,gatherbuddy,etc) gives control to CLU.

* Target Priority:	
- If we have a RaF leader, then use its target.
- Healers first (Battlegrounds/arenas)
- Enemy’s  Attacking Us  (Battlegrounds/arenas)
- Flag Carrier units (Battlegrounds/arenas)
- Low Health units (Battlegrounds/arenas)
- bot poi.
- Target list (Honorbuddy Targeting)
- MostFocusedUnit (Targets the unit that everyone else in your party is focused on, or targets the closest target to you.)
		

Information
---------------------

* Target
- Information on the current target includes Location, Distance, Name, GUID, isBehind.

* Spells
- Dumps the target spells (including ID's, Powercost, etc) to the honorbuddy debug log

* Auras
- Dumps the targets Auras to the honorbuddy debug log

* Spellchecker
- provides information on the spell you provide (spellname or spell Id)

Keybinds
------------------

* With Sound Notification
- PauseRotation
- UseCooldowns
- ClickExtraActionButton
- UseAoEAbilities
- EnableRaidPartyBuffing
- HealEnableSelfHealing
- EnableMultiDotting
- EnableInterupts
- ChakraStanceSelection

Statistics
------------------

* Spells cast
* Average APM
* Spell Cast intervals
* Healing report (Spell casts that succeeded)

User Interface
------------------------

* Settings
- General
- Class Specific

* Healing Helper
- Blacklisting Players
- Urgent Dispel selector
- MainTank/Offtank Selection
- Beacon of Light Selection
- Earth Shield Selection
- LifeBloom Selection
- Role Selection (Tanks/Healers/Damage)
- Add/Remove Members

* Debugging
- Target information
- Healing Target Information
- Reports
- Aura/Spell Dumps
- Spellchecker

* Spell Lock Watcher
- Watchs debuffs on your target and counts down when time remaining is less than three seconds.

* Realtime Target Information
- Information on the current target includes Location, Distance, Name, GUID, isBehind.
		

I want more information show me the good stuff!!
============================================

Full Documentation can be found your reading it!


Where is the Change Log? How do I know what is updated?
===================================================

Change Log as well as a rolling list of changes can be found https://code.google.com/p/clu-for-honorbuddy/source/list

How to report a bug
==================
* Make sure you are using latest version! - Update your via SVN regularly.
* Verify that you have followed all the steps listed under the 'Installation'
* Please enable Debug Logging within CLU settings before you attach your log..the more information the better.
* Attach a log (See How to by Kickazz006) http://www.thebuddyforum.com/honorbuddy-forum/guides/35945-guide-how-attach-your-log-kick.html


Credits
=======
* cowdude for his initial work with Felmaster and giving me inspiration to create this CC.
* All the Singular Developers For there tiresome work and supporting code
* Weischbier for his Support with the new 5.0.4 MoP Changes.
* bobby53 For his Disclaimer (Couldn't have said it better)
* Kickazz006 for his BlankProfile.xml
* Shaddar & Mentally for thier initial detection code of HourofTwilight, FadingLight and Shrapnel
* bennyquest for his continued efforts in reporting issues!
* Jamjar0207 for his Protection Warrior Rotation and Fire Mage
* gniegsch for his Arms Warrior Rotation and improvements with blood deathknight
* Stormchasing for his help with warlocks and patchs
* Obliv For his Boomkin, Frost Mage & Assassination rotations and Arms warrior improvement
* ShinobiAoshi for his initial Affliction warlock rotation
* fluffyhusky for his initial Enhancement Shaman rotation
* Digitalmocking for his initial Elemental Shaman rotation
* Toney001 for his improvements to the Unholy Deathknight rotation
* kbrebel04  for his improvements to Subtlety Rogue
* alxaw (HB9806B90) for his initial Arms rotation
* Dagradt for his PvP rotations.
* LaoArchAngel for his help with core changes and Rogue changes.
* Condemned for Mage Frost Update
* Apoc for just about everything :P


Development Team
==================

- Wulf
- Stormchasing
- Weischbeir
- LaoArchAngel
- Dagradt
- Ama
- handnavi
- TuanHA



Disclaimer
=========
Use of 3rd party programs are against Blizzard's terms of use and license.
This CC is not intended for use on live Blizzard Entertainment World of Warcraft realms.
Any use on Blizzard realms is at your own risk and not supported per this statement.


F.A.Q
======

Q. CLU crashes with Unable to find Active Rotation OR System.Reflection.ReflectionTypeLoadException
A. Update to Microsoft .Net 4

Q. Can I use this CC for low level questing/grinding/GB2/BGBuddy?
A. CLU was designed for High level Raiding - having said that it is capable of performing movement, targeting, buffing, combat from level 20-90.
*Note: Using it for these functions are not fully supported. If it's not working they way you want..its for a reason..we just do not want to support 
	   low level combat currently. Please use Singular for this, it ships free with Honorbuddy.

Q. Can CLU tank raids and heroic dungeons ?
A. All Tank/Healing specs provided have been tested for Heroics/Raids.

Q. Will CLU taunt targets for me when tanking?
A. No CLU does not taunt for you.

Q. What Botbase should I use for Raiding/ Heroic Dungeons?
A. Depends on what you want to do. CLU works best with Raidbot but Lazyraider and Combat can be used as well. When Healing Use Lazyraider and turn of Tank selection (aka: Solo Mode)

Q. Where is XYZ setting?
A. If you do not see it in the UI settings it's not there yet!

Q. Is there any way for me to have control over blade flurry as a rogue? 
A. Turn off AoE, this is true for all class AoE abilities.

