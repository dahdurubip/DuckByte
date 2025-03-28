#### v2.2.1:

- Fixed asset payment dialog
- List support in asset query for versions

### v2.2.0:

- Global avatar style selection (Tools -> UnionAvatars -> Project Setup -> Settings)
- Merged Creation and Update UI into a single modue (UI Sample)
- HDRP shadow warnings fixed

### v2.1.0:

- Cartoon style avatars
- Hair color selection
- Bug fixes and optimizations

#### v2.0.3:

- Fixed authentication bug that forced usage of API key instead of user token
- Fixed issue when updating an avatar with neutral gender

#### v2.0.2:

- Fixed camera angle issue in mobile devices (deformed avatars)
- Updated GLTFast and Draco dependencies to Unity's fork
- Added a project setup window in the Editor tool

#### v2.0.1:

- Made organization a mandatory parameter for Session
- Added organization setup to edior window
- Fix API key generation 

## v2.0.0:

- Updated API to v2
- New avatar generator version
- Added asset Wardrobe
- Improved user experience and error handling
- Improved Editor Window
- Improved samples
- Removed SuperAuth
- Removed IFrame sample
- Bug fixes and optimizations


#### v1.5.3:

- Fixed issue in WebGL URP builds where webcam images where being darkened
- Fixed the Trobuleshooting link

#### v1.5.2:

- Added overload to ImportAvatarAsHumanoidLOD. Now it supports loading of a single LOD level
- Updated DracoUnity to 4.1.0
- Fixed Avatar View scaling in UI
  
#### v1.5.1:

- Fixed WebCamTexture issue Unity 2022 WebGL builds
- Fixed issue with LOD avatar importing
- Fixed default bodies in avatar creation
- Added filtering of version and style for garments and outfits
- Other minor fixes

### v1.5.0:

- Refactored pose conversion and improved animations
- Added VR support with User Interface (see docs)
- Added default garments during customization
- Minor bug fixes

### v1.4.0:

- Refactored Resource Downloader
- Added support for LOD
- Avatar cache improvements

#### v1.3.3:

- Added support for v3 bodies
- Fixed minor bug in avatar delete button

#### v1.3.2:

- Fixed issue where linker wasn't being used during builds
- Improved resource cache
- Fixed avatar blinking animation
- Fixed bug when deleting an avatar during the selection
- Downgraded draco to 3.4.0
- Added NPC sample to package

#### v1.3.1:

- Hotfixes for Avatar Update and gender selection
- Temporary disabled the WalletConnect button in the UI

### v1.3.0:

- Implementation of Avatar Inventory (Garments, Hairs, Brands and Collections)
- Added name checking in avatar creation
- Other minor fixes

### v1.2.0:

- Implementation of Developer Credentials (see docs)

### v1.1.0:

- Support for Avatar Optimization
- Minor fixes to sample animations

## V 1.0.0:

- Migration to Unity Packages
- Swapped from GLTFUtility to GLTFast
- AvatarImporter methods are now async
- Usage analytics
- Moved examples into package samples
- SuperAuth (Experimental)
- WalletConnect integration
- New avatar creation UI