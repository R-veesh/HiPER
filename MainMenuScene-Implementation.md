# MainMenuScene Implementation Guide

## 1) MainMenuScene UI
- Open `MainMenuScene` in Unity.
- Inside the Canvas, create 3 panels:
  - `ModeSelectPanel`
  - `LanPanel`
  - `OfflinePanel`
- In `ModeSelectPanel`, add these buttons:
  - `Offline Game`
  - `LAN Game`
  - `Store`
  - `Profile`
  - `Quit`
- In `LanPanel`, add:
  - `Host`
  - `Join`
  - `Back`
  - IP input field
  - status text
- In `OfflinePanel`, add:
  - `Play`
  - `Next Map`
  - `Prev Map`
  - `Next Car`
  - `Prev Car`
  - map name text
  - map description text
  - difficulty text
  - progress text
  - car name text
  - status text
  - owned cars text
  - map preview image
  - car preview image
  - `Back`

## 2) MainMenuManager setup
- Attach `Assets/resource/MainMenuScene/MainMenuManager.cs` to a scene object in `MainMenuScene`.
- Assign these fields in the Inspector:
  - `modeSelectPanel` -> `ModeSelectPanel`
  - `lanPanel` -> `LanPanel`
  - `offlinePanel` -> `OfflinePanel`
  - all related buttons, texts, and images
- Set `storeUrl` to your real web store URL.

## 3) CustomNetworkManager setup
- Select the NetworkManager object using `Assets/resource/MainMenuScene/CustomNetworkManager.cs`.
- Assign:
  - `offlineChallengeMaps`
  - `lobbyPlayerPrefab`
- Check scene names:
  - `mainMenuScene`
  - `lobbyScene`
  - `gameScene`

## 4) Challenge maps
- Create `MapData` assets using `Assets/resource/LobbyScene/MapData.cs`.
- For each map, set:
  - `mapName`
  - `mapDescription`
  - `mapPreview`
  - `sceneName`
  - `difficulty`
  - `laps`
- Put them in `offlineChallengeMaps` in the challenge order.
- Map at index `0` becomes the default unlocked map.

## 5) Offline car setup
- In `MainMenuManager`, fill:
  - `offlineCarNames`
  - `offlineCarPreviewSprites`
- Keep this order matched with your car prefab order used in gameplay.
- Car index `0` is treated as the default owned car.

## 6) Profile summary panel
- Attach `Assets/resource/MainMenuScene/ProfileSummaryPanel.cs` to your profile panel object.
- Assign:
  - `panel`
  - `displayNameText`
  - `emailText`
  - `levelText`
  - `matchStatsText`
  - `coinText`
  - `ownedCarsText`
  - `statusText`
  - `editButton`
  - `closeButton`
  - `refreshButton`
- Assign `profileEditPanel` to the object that has `ProfileUI`.

## 7) Profile edit UI
- Open the object with `Assets/resource/LoginScene/ProfileUI.cs`.
- Assign the new field:
  - `profileImageUrlField`
- Also verify these are assigned:
  - display name input
  - age input
  - bio input
  - status text
  - save button
  - close button
  - panel

## 8) Race result UI
- In the race scene, select the object using `Assets/resource/MainGameScene/RaceResultUI.cs`.
- Assign:
  - `nextChallengeButton`
  - `progressText`
- This is used to show offline progression and the next challenge button after a win.

## 9) Build Settings
- Make sure these scenes are added to Build Settings:
  - `MainMenuScene`
  - `LobbyScene`
  - all offline challenge race scenes

## 10) Test flow
- Log in.
- Open main menu.
- Click `Offline Game`.
- Check map change works.
- Check car change works.
- Click `Play`.
- Win the race.
- On result screen, verify:
  - progress text updates
  - level/win progression updates
  - `Next Challenge` button appears
- Then test `LAN Game` and confirm host/join still works.

## Important scripts
- `Assets/resource/MainMenuScene/ChallengeProgressService.cs`
  - handles local progression save
- `Assets/resource/MainMenuScene/OfflineRaceConfig.cs`
  - carries selected map/car into race scene
- `Assets/resource/MainMenuScene/PlayerDataContainer.cs`
  - feeds offline spawn data
- `Assets/resource/LoginScene/UserSession.cs`
  - now stores level, wins, owned cars, profile image URL

## Current limitation
- Full backend store/inventory sync is not finished yet.
- Right now, offline progression is working with local save.
- If you want full web unlock sync, the next step is adding inventory/profile/progression API integration.
රි, මේ bug එකේ root cause එක clear:
ProfileSummaryPanel එකේ UpdateView() තුල UserSession.Instance == null උනොත් statusText = "No user session" set වෙනවා (Assets/resource/MainMenuScene/ProfileSummaryPanel.cs:69).  
ඒ නිසා profile data fields පුරවෙන්නෙ නෑ.
Quick fix (Unity Inspector + run flow) first:
- LoginScene එකේ එකම GameObject එකකට මේ 3 scripts තියෙන්න ඕන:
  - UserSession
  - AuthManager
  - ApiClient
- Game run කරන්න LoginScene එකෙන් (MainMenu scene direct play නොකර).
- Login success වෙලා MainMenuSceneට ගියාම UserSession data තිබෙනවා.
- MainMenuScene එකේ ProfileSummaryPanel references full assign කරලාද check කරන්න:
  - displayNameText, emailText, levelText, matchStatsText, coinText, ownedCarsText, statusText
- MainMenuManager එකේ profileSummaryPanel field assign කරලාද බලන්න.
- Backend login response එකේ fields match වෙන්න ඕන:
  - user.id, user.email, user.displayName, token (required)
Most common reason in your case:
- Unity play button press කරන scene එක MainMenuScene නම් UserSession create වෙලා නැතිව profile panel open වෙනවා.
මම දැන් code-level hard fix plan එකත් ready (auto bootstrap so MainMenu direct run වුනත් session services create වෙන විදිහට) —  
targeted question: ඔයා generally Play කරන්නේ MainMenuScene එකෙන්ද, LoginScene එකෙන්ද? (Recommended: LoginScene)

හරි, No user session වැටෙන්නේ UserSession.Instance null නිසා.  
ඔයා login වුනාට පස්සේ MainMenu එකට යද්දී session object එක survive වෙන්නේ නැති case එකක්.
මේක first fix කරන්න මේ steps follow කරන්න:
- LoginScene එකේ root-level GameObject එකක් හදන්න (SessionBootstrap)
- ඒ object එකට scripts 3ක් දාන්න:
  - UserSession
  - AuthManager
  - ApiClient
- මේ SessionBootstrap object එක Canvas child එකක් නොවෙන්න (root එකේම තියෙන්න ඕන)
- MainMenuScene එකේ duplicate UserSession/AuthManager/ApiClient objects තිබ්බොත් remove කරන්න
- Game run කරද්දී login කරලා MainMenu එකට ගියාම Hierarchy වල DontDestroyOnLoad section එකේ SessionBootstrap තියෙනවද බලන්න
තව check 2ක් කරන්න:
- MainMenuManager inspector එකේ profileSummaryPanel assign කරලා තියෙනවද
- ProfileSummaryPanel inspector එකේ text fields (displayNameText, emailText, levelText...) assign කරලා තියෙනවද
ඇත්තම root issue එක normally මේක:
- LoginScene එකේ session scripts child object එකක තියෙනවා
- scene change වෙද්දි destroy වෙනවා
- profile panel open කරද්දී No user session
මම next step එකට code-level hard fix එක plan කරලා තියෙනවා (session lost වුනත් auto recover වෙන විදිහට).  
ඔයා confirm කරන්න: ඉහත root-object setup කරාට පස්සේත් issue තියෙනවද?

හරි, මේ screenshot-like list එකෙන් issue එක confirm වෙනවා:
- DontDestroyOnLoad යට UserSession / AuthManager / ApiClient නැහැ
- ඒ නිසා ProfileSummaryPanel එක No user session කියන එක correct behavior
ඒ කියන්නේ login වෙලාත් session object MainMenu scene එකට carry වෙන්නේ නැහැ.
Fix path (no code edit, Unity setup only):
- LoginScene එකේ root object එකක් හදන්න: SessionBootstrap
- ඒකට scripts 3 assign කරන්න:
  - UserSession
  - AuthManager
  - ApiClient
- object එක inactive නොවෙන්න
- MainMenuScene එකේ මේ scripts තියෙන duplicate objects remove කරන්න (තිබ්බොත්)
- Play mode run කරලා login කරලා MainMenu යන්න
- Hierarchy DontDestroyOnLoad යට දැන් මෙන්න මේවා බලන්න:
  - SessionBootstrap (UserSession, AuthManager, ApiClient)
  - NetworkManager
  - PlayerDataContainer
  - ChallengeProgressService
  - OfflineRaceConfig
Quick verify:
- login success වෙන තැන UserSession.Instance.SetFromLoginResponse(...) call වෙනවා
- MainMenu profile open කරද්දී No user session නැතුව DisplayName/Email populate වෙන්න ඕන
ඔයාට දැන් කරලා බලලා result කියන්න.  
එහෙමත් නැත්නම් next step එකට මම exact Inspector-by-Inspector troubleshoot checklist (10 checks) දෙන්නම්.  