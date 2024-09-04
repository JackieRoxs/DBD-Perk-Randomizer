# Dead By Daylight: Perk Randomizer

This project is a more modern version of the past perk randomizer websites and Discord Bots.
This uses the Search Bar they added back in mid 2023 to help automatically equip the perks for you.

## Warning
BHVR has not given an opinion on this specifically, but this does not even read the game's files or screen. Simply moves the mouse and types the perk name for you, so if anything it'd be considered a "macro"
[BHVR's Opinion on Macros]([BHVR's opinion on macros](https://forums.bhvr.com/dead-by-daylight/discussion/82667/will-struggle-macros-get-you-banned)) is **"at your own risk"** which is the same statement they give for [Custom Icons, Portraits etc](https://steamcommunity.com/app/381210/discussions/0/5291222404438553872/#c5291222404440824189). For BHVR, "at your own risk" means they will not ban any player themselves, but don't go to them if EAC does. 

## How to Download
* Download the latest from [Release](https://github.com/JackieRoxs/DBD-Perk-Randomizer/releases/tag/Release)
* Extract the Zip to Location of your choice
* Run **DBD-Randomizer.exe**

## How To Use
* Upon Launching, first you need to press "*Update Perk List*"
    * It will give you a message when it successfully downloaded the latest perks from Nightlight
* Click *"Calibration"*
    * Calibration page will then have a *"Start Calibration"* Button for you to use
    * After starting it, you will recieve message on the top right of your screen of what to do to calibrate.
    * The purpose of calibration is that every player's setup is unique, from screen size and resolution to UI scaling preferences. To ensure the program works seamlessly on your specific setup, we provide a calibration process. This process will guide you through clicking on key elements like perk slots and the search button, recording their positions for future use. By calibrating, you enable the program to automate actions accurately, tailored to your individual settings.
* Optionally
    * Enter *"Settings"* to select what Hot Keys you'd like to randomize Killer or Survivor perks respectively. 
* To Start using the randomizer, enter the *"Randomizer"* Page, you will have a button to randomize killer or survivor. You also have a color coded button to enable to disable hotkeys. 
    * If you are using one monitor, it's suggested to do the optional hotkeys, if you don't want to, make sure to move the Application Window below the perk slots to prevent the application from clicking on itself. 

## Future Features
* Perk Filter
    * I was thinking of making a light-weight version for 1.0, but I will delay feature till 1.1 or 1.2 so I can have a whole UI with perk icons. 
* Survivor Items
* Better Visuals
    * I know how to code, I am bad at design
* Duplicate Perk Fix
    * Currently if the randomizer tries to equip a perk that you already have equipped, it will unequip. 

## Limitations
* Add-Ons
    * Specifically Killer-Add Ons. I currently lack a database or API containing every killer add-on that reliablies updates. 
    * Automatically Detecting Killer for Add-Ons
        * While it is possible to add a few more calibrations to set a range for an OCR to read the killer's name, issue is that the Lobby Changes. As of right now, Castlevania themed lobby has a very white top of lobby screen. White letters on white background makes it extremely hard for an OCR to identify killer. 

## Support & more
* [Discord](https://discord.gg/hSGpY3qV6F).

## Credits
* JackieRoxs: Developer
* [Nightlight](https://nightlight.gg/).: API Source for perk data 
