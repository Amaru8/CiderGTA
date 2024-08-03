# CiderGTA

## About

CiderGTA is an external integration for the community focused Apple Music client, Cider. For more info on [Cider](https://cider.sh), head over to [cider.sh/learn-more](https://cider.sh/learn-more)!

## Installation

### Requirements

- The official game, licensed. Usage with pirated copies is not recommended and no support will be offered.
- Latest ScriptHookV and ScriptHookVDotNet installed to your game.
- Basic modding knowledge.
- OpenIV, or a similar tool you are comfortable with.
- Cider, at least on version 2.5.
- [Community HUD.gfx](https://www.gta5-mods.com/tools/community-hud-gfx-for-add-on-radio-stations) mod installed to your game.

### Steps

- Open the [GitHub Releases page](https://github.com/Amaru8/CiderGTA/releases/latest) and click on `Assets` at the bottom to download the latest version. You usually need to look for a file where the name ends with `.zip`.
- Extract the downloaded archive.
- Copy the extracted `scripts` folder to the main directory of your game installation folder.
- Navigate to `game installation folder \ mods \ update \ x64 \ dlcpacks` and insert the `ciderradio` folder from the DLC directory.
- Append `<Item>dlcpacks:/ciderradio/</Item>` to `dlclist.xml` in `game installation folder \ mods \ update \ update.rpf \ common \ data`.
- Open `game installation folder \ mods \ update \ update.rpf \ x64 \ data \ cdimages \ scaleform_generic.rpf \ hud.ytd` in Edit Mode after you installed the Community HUD.gfx mod.
- Select `gtav_radio_stations_texture05_512` in the Texture Editor and replace it with the `community_with_cider_icon.dds` file from the extracted mod folder.
- Enable external connectivity by opening `Settings > Connectivity > External Applications` and turning on the **WebSockets API** and the **RPC Server**.
- Create a new API key by clicking `Manage External Application Access > Create New` and replace `PLACEHOLDER` in the `CiderGTA.ini` file with the generated token.

## Third-Party Software

**Rockstar Games and Take-Two Interactive** [Link](https://www.rockstargames.com/gta-v)

**Community HUD.gfx** [Link](https://www.gta5-mods.com/tools/community-hud-gfx-for-add-on-radio-stations) 

**ScriptHookV** [Link](https://dev-c.com/GTAV/scripthookv)

**ScriptHookVDotNet** [Link](https://github.com/scripthookvdotnet)

**LemonUI** [Link](https://github.com/LemonUIbyLemon/LemonUI)

**SocketIOClient** [Link](https://www.nuget.org/packages/SocketIOClient)

Thanks to all developers and people working for these projects.

## Special Thanks

Big thanks to [sahailee](https://github.com/sahailee) for his awesome [Spotify Radio mod](https://github.com/sahailee/GTA-Spotify-Radio), which served as a huge inspiration and base for this project.

## Note

This mod also uses the `RADIO_47_SPOTIFY_RADIO` station included in Community HUD.gfx. This means that CiderGTA and sahailee's Spotify Radio can't be used simultaneously.
If you know a direct way to contact the author of the GFX.mod, [WildBrick142](https://www.gta5-mods.com/users/WildBrick142), please send me a mail to [amaru@cider.sh](mailto:amaru@cider.sh).

## Disclaimer

_This project is NOT affiliated with Apple, Rockstar Games or Take Two Interactive in any way, shape, or form. Cider Collective, or other developers and contributors of Cider Collective Software are not related to this project. It is open source and free to use (if the requirements are fulfilled). For any legal requests, contact me at [amaru@cider.sh](mailto:amaru@cider.sh)._
