# Shift At Midnight - full class map

Generated from the Il2Cpp interop assembly (fields + method signatures) for every game class.
Compiler-generated members (<>c, MoveNext, PDM invokers) are omitted.

## class AStarDoorGraphUpdater : MonoBehaviour
- fields: doorCollider padding graphIndex 
- methods: CloseDoor(); OpenDoor(); SetDoorOpen(bool open); 

## static class Achievements : Il2CppSystem.Object

## class ActorCOMTransform : MonoBehaviour
- fields: offset actor 
- methods: Update(); 

## class ActorSpawner : MonoBehaviour
- fields: template maxInstances spawnDelay instances timeFromLastSpawn 
- methods: Update(); 

## class AdaptiveFovByAspect : MonoBehaviour
- fields: normalFov squashedFov referenceAspect _cam _lastW _lastH 
- methods: ApplyFovIfNeeded(); Awake(); OnDisable(); OnEnable(); 

## class AddRandomVelocity : MonoBehaviour
- fields: intensity 
- methods: Update(); 

## class AmbientMusicSystem : MonoBehaviour
- fields: ambientTracks curTrackIndex inHunt pauseAmbience 
- methods: Awake(); FixedUpdate(); Start(); SwitchTracks(); 

## class AnimationEventTrigger : MonoBehaviour
- fields: animEvent animEvent2 animEvent3 animEvent4 teleportPosition cam 
- methods: ExecuteEvent(); ExecuteEvent2(); ExecuteEvent3(); ExecuteEvent4(); GoBackToPlayerCam(); MatchCameraFOV(); Start(); SwitchCamera(GameObject cam); TeleportPlayer(); 

## class AudiencePath : WalkPath
- fields: angle peopleRotation looking target damping 
- methods: DrawCurved(bool withDraw); SpawnPeople(); 

## class AudioVolumeSliders : MonoBehaviour
- fields: musicSlider sfxSlider musicValue sfxValue audioMixer MusicPrefsKey SFXPrefsKey MusicVolumeParameter SFXVolumeParameter k_soundOffDBValue 
- methods: LoadAudioSettings(); OnDestroy(); SetMusicVolume(float volume); SetMusicVolumeInternal(float volume, bool save); SetSFXVolume(float volume); SetSFXVolumeInternal(float volume, bool save); Start(); 

## class AutoStartObjective : MonoBehaviour
- fields: id key 
- methods: Start(); WaitForStoreManager(); 

## class AutosaveScreen : MonoBehaviour
- fields: k_eulaSceneName k_transitionDelay 
- methods: Start(); 

## class BabyDoll : Enemy
- fields: spiderSpawn patrolAreas huntMan seeker pathfinder normalSpeed runSpeed annoyedSpeed rampageSpeed timeAtPatrol curSpeed walkTarget barricadeTarget scentTarget playerScentTarget chaseTarget chasingTarget breakingBarricade justAttackedBarricade attackBarricadeCooldown waitingAtPatrolTime alreadyStartedNextPathway justStartedPatrol canAttack chasingNonPlayerObject attackAudio roarAudio interestedAudio jumpAudio justDetectedPlayer dollAnim playerMans detectionOfEachPlayer timeChasingObject timeChasingBeforeGivingUp beingHit oneCreature thisPlayer enemyHolder fastPacing chasingPlayer chasingPlayerTarg jumpingAtPlayer died justStartedTelegraph actuallyJumping timeJumping dealtDamage shotProj jumpObstacles collider rb lookAtPlayer disappearParticle lookAtTransform detectsBeforeLookForJumpPoint fastPacingButDontChasePlayer _NetworkedPosition _NetworkedRotation _IsPositionInitialised _hasSnappedToInitialPosition 
- methods: ChangePlayerLookAtState(PlayerManager playerMan, bool lookAt); ChaseNonPlayerTarget(Vector3 targPosition); CheckEnemiesLeft(float timeUntilCheck); CheckIfNearBarricade(); CompleteHunt(); CopyBackingFieldsToState(bool A_1); CopyStateToBackingFields(); DetectPlayers(); Die(); FixedUpdate(); FixedUpdateNetwork(); GoToBreakBarricade(); GoToPatrol(); GoToTarget(); Hit(); JumpAtPlayer(); Render(); Rpc_CMD_ChaseNonPlayerTarget(Vector3 targPosition); Rpc_CMD_Die(); Rpc_ChangeHittableHealth(float health); Rpc_ChangePlayerLookAtState(PlayerRef playerRef, bool lookAt); Rpc_ChaseNonPlayerTarget(Vector3 targPosition); Rpc_Die(); Rpc_PlayRoar(); Rpc_SpawnDisappearParticle(Vector3 pos); Spawned(); Start(); StartNextPathway(); TPToRandomPatrolPoint(); UpdatePlayerLists(); 

## class BackButtonPressed : MonoBehaviour
- fields: playerInput objectsToShowOnBack objectsToHideOnBack buttonsToClick objToHighlightOnBack controllerDisconnectPopup 
- methods: Awake(); BackPressed(); OnBackButtonPressed(InputAction.CallbackContext context); OnDisable(); OnEnable(); 

## class Barricade : ConstrictedInteractable
- fields: colliderHolder barricade doorAnim door entryDoor plankDrop hittable 
- methods: BarricadeDestroyed(); CheckForCurItem(int curIndex); CopyBackingFieldsToState(bool A_1); CopyStateToBackingFields(); EnableInteractable(); EnableInteractableAfterDelay(); Place(); Rpc_CMD_Place(); Rpc_Place(); 

## class BathroomGuy : MonoBehaviour
- fields: doorAnim loopingAudio openDoorSfx 
- methods: OpenDoor(); Start(); 

## class BearTrap : NetworkBehaviour
- fields: trapAnim monsterCheckScript caught cantPlaceRadius curCantPlaceRadius radiusPos interactable saveSnapshotObject 
- methods: CopyBackingFieldsToState(bool A_1); CopyStateToBackingFields(); DisableRadius(); EnableRadius(); Rpc_CMD_Trap(); Rpc_Trap(); Start(); Trap(); 

## class BetaManager : MonoBehaviour
- fields: beta objsToEnableInBeta objsToDisableInBeta 
- methods: Start(); 

## class Blinker : MonoBehaviour
- fields: highlightColor rend original 
- methods: Awake(); Blink(); LateUpdate(); 

## class BrowserLobbyData : Il2CppSystem.Object
- fields: lobbyId lobbyName passwordProtected gamemode night playerCount maxPlayerCount latency 

## static class BuildInfo : Il2CppSystem.Object
- fields: Version 

## class CFX_AutoDestructShuriken : MonoBehaviour
- fields: OnlyDeactivate 
- methods: CheckIfAlive(); OnEnable(); 

## class CFX_AutoStopLoopedEffect : MonoBehaviour
- fields: effectDuration d 
- methods: OnEnable(); Update(); 

## class CFX_Demo_RandomDir : MonoBehaviour
- fields: min max 
- methods: Awake(); 

## class CFX_Demo_RotateCamera : MonoBehaviour
- fields: rotating speed rotationCenter 
- methods: Update(); 

## class CFX_Demo_Translate : MonoBehaviour
- fields: speed rotation axis gravity dir 
- methods: Start(); Update(); 

## class CFX_LightIntensityFade : MonoBehaviour
- fields: duration delay finalIntensity baseIntensity autodestruct p_lifetime p_delay 
- methods: OnEnable(); Start(); Update(); 

## class CFX_SpawnSystem : MonoBehaviour
- fields: instance objectsToPreload objectsToPreloadTimes hideObjectsInHierarchy spawnAsChildren onlyGetInactiveObjects instantiateIfNeeded allObjectsLoaded instantiatedObjects poolCursors 
- methods: Awake(); GetNextObject(GameObject sourceObj, bool activateObject = true); PreloadObject(GameObject sourceObj, int poolSize = 1); Start(); UnloadObjects(GameObject sourceObj); addObjectToPool(GameObject sourceObject, int number); increasePoolCursor(int uniqueId); removeObjectsFromPool(GameObject sourceObject); 

## class CamShakeEvent : MonoBehaviour
- methods: Shake(float intensity); 

## class CameraFOVChanger : MonoBehaviour
- methods: OnEnable(); 

## class CameraManager : MonoBehaviour
- fields: ENABLE_DEBUG_PLATFORM _PreviousCamera m_bInitialized 
- methods: Awake(); CreateManager(); DisableActiveCamera(); GetActiveCamera(); SetActiveCamera(GameObject newCam); ShouldIgnoreCamera(GameObject camObj); 

## class CameraShake : MonoBehaviour
- fields: intensity usesDuration duration originalPos justFinishedIntensity 
- methods: FixedUpdate(); LerpToNormal(); OnEnable(); 

## class CameraTranslation : MonoBehaviour
- fields: sensitivity maxOffset initialPosition started lerpSpeed offsetX offsetY offsetXChangesZ 
- methods: OnDisable(); OnEnable(); Update(); 

## class CamouflagedMonsterRun : MonoBehaviour
- fields: anim goal moveSpeed turnSpeed snapDistance snappedToGoal 
- methods: Finish(); FixedUpdate(); OnEnable(); SnapToGoal(); 

## class CanBeHitByTrap : MonoBehaviour
- fields: hittable 
- methods: OnTriggerEnter(Collider otherCol); 

## class CanEnterDoor : MonoBehaviour
- fields: enemy cantEnterFrontDoor 
- methods: OnTriggerEnter(Collider other); 

## class Car : NetworkBehaviour
- fields: spawnsNextNpc carAnim matFaders objsToTurnOffWhenFade carEngineRunning carDoorInteractables carDoorAnims npc petrolTank carLicencePlate localizedCarLicencePlate carDescriptionQuestion fadeOutAudio tutorialArrow carColliders licensePlate alreadyFaded hasTriggeredNextNpc carInspectables petrolTankOpen leaveBtn howMayIHelpBtn clothesBtn makeThisCarTheSingleton 
- methods: Awake(); CarDescriptionEnabled(); CarDestroy(); CarDone(); CarFadeAway(); CarLeave(); CarUnlock(); CloseAllDoors(); CopyBackingFieldsToState(bool A_1); CopyStateToBackingFields(); DisableAllInteractables(); FixedUpdate(); InteractWithNPC(); NewQuestioningTopicHint(); Rpc_CMD_CarDone(); Rpc_CMD_CarFadeAway(); Rpc_CMD_CarUnlock(); Rpc_CMD_InteractWithNPC(); Rpc_CarDone(); Rpc_CarFadeAway(); Rpc_CarUnlock(); Rpc_CloseAllDoors(); Rpc_InteractWithNPC(); Start(); TogglePetrolTankOpen(); 

## class CarComputer : Interactable
- fields: name values key value entries searched newCam interacting stopInteractEvent otherComputerCanvas playerInput regoText descObj emptyScrollView noResultsScrollView resultsWindow dBNames searchField curDBName inputText 
- methods: Awake(); CleanSearchText(string text); CopyBackingFieldsToState(bool A_1); CopyStateToBackingFields(); Interact(PlayerManager playerMan); LoadDbNames(); Rpc_CMD_SearchOnAllClients(string s); Rpc_SearchOnAllClients(string s); Search(); SearchOnAllClients(); SetDescValues(string plateInput); StopInteract(); Update(); 

## class CarTutorialManager : NetworkBehaviour
- fields: tutorialComplete askedToHelp searchedCarWithFlashlight searchedLicensePlate tickedCheckbox askedToHelp_ searchedCarWithFlashlight_ searchedLicensePlate_ canvas tellThemToLeaveHolder leaveBTNHolder amountOfObjectivesDone 
- methods: AskToHelp(); Awake(); CheckIfAllObjectivesDone(); CopyBackingFieldsToState(bool A_1); CopyStateToBackingFields(); Rpc_AskToHelp(); Rpc_CMD_AskToHelp(); Rpc_CMD_SearchLicensePlate(); Rpc_CMD_SearchedCarWithFlashlight(); Rpc_CMD_TutorialComplete(); Rpc_SearchLicensePlate(); Rpc_SearchedCarWithFlashlight(); Rpc_TutorialComplete(); SearchCarWithFlashlight(); SearchLicensePlate(); 

## class ChangePitchPerLoop : MonoBehaviour
- fields: audio maxPitch minPitch dontLoop justStopPlaying changePitchAtStart 
- methods: FixedUpdate(); ForceChangePitch(); OnEnable(); 

## class ChangeTextToKeybind : MonoBehaviour
- fields: surroundedByParentheses uppercase keybindNum playerInput glyphManager k_spriteName circleIcon 
- methods: Awake(); OnControlsChanged(PlayerInput input); OnDisable(); OnEnable(); Refresh(); 

## class CharacterControl2D : MonoBehaviour
- fields: acceleration maxSpeed jumpPower floorRaycastDistance unityRigidbody 
- methods: Awake(); FixedUpdate(); Update(); 

## class CharacterDeathMemory : MonoBehaviour
- fields: charName skipIfDead 
- methods: Kill(); Start(); 

## class ChaseNearestPlayer : NetworkBehaviour
- fields: endChaseWhenEveryoneInside jamGun bodyCollider matFaders anim startAnim chaseAnim attackRange runSpeed ended seeker pathfinder nearestPlayer oldNearestPlayer attacking _NearestPlayerRef timeToStartChase rb lookAtPlayerHead chasing damageToPlayer _NetworkedPosition _NetworkedRotation 
- methods: CopyBackingFieldsToState(bool A_1); CopyStateToBackingFields(); DealDamageToPlayer(); Delete(); FinishAttack(); FixedUpdate(); FixedUpdateNetwork(); Render(); Rpc_Attack(NetworkObject target); Rpc_EndChase(); Rpc_UpdateNearestPlayer(NetworkObject obj); Spawned(); Start(); StartActualChasing(); 

## class ChaseWhenNear : NetworkBehaviour
- fields: chaserObj finishCanvas chaseAudio carDisappearAnim spawnChasingNathanPos runInsideCanvas alreadyStartedChase chasing chaseStartTrigger 
- methods: Awake(); CopyBackingFieldsToState(bool A_1); CopyStateToBackingFields(); FixedUpdate(); Rpc_CMD_StartChase(); Rpc_EndChase(); Rpc_StartChase(); StartChase(); 

## class ChatLogNode : MonoBehaviour
- fields: nameText subtitleText correct incorrect anim 
- methods: Disappear(); RevealTextUniversal(); Start_(); 

## class CheatsMenu : NetworkBehaviour
- fields: allowCheatsEvenIfReleaseBuild godMode money speed godModeOff godModeOn decryptOff decryptOn cheatsMenuOpen cheatsMenu customerIdText customerIdFound customerIdNotFound playerInput pauseButtonDown moneyButtons micIconCanvas hud 
- methods: AddMonsterToSpawnQueue(int monsterSpawn); AddShopRefresh(); Awake(); ChangeDecryptText(bool on); ChangeGodMode(bool on); ChangeMoney(float moneyChange); ChangeSpeed(float newSpeed); CleanId(string s); CopyBackingFieldsToState(bool A_1); CopyStateToBackingFields(); ExitCheatMenu(); Rpc_AddMonsterToSpawnQueue(int monsterSpawn); Rpc_CMD_AddMonsterToSpawnQueue(int monsterSpawn); Rpc_CMD_ChangeMoney(float moneyChange); Rpc_CMD_ChangeSpeed(float newSpeed); Rpc_CMD_SkipToCertainDay(int day); Rpc_CMD_SkipToNextDay(); Rpc_CMD_SpawnCow(); Rpc_CMD_SpawnCustomer(string custId); Rpc_CMD_StartHunt(); Rpc_ChangeMoney(float moneyChange); Rpc_ChangeSpeed(float newSpeed); Rpc_SkipToCertainDay(int day); Rpc_SkipToNextDay(); Rpc_SpawnCow(); Rpc_SpawnCustomer(string custId); Rpc_StartHunt(); SkipToCertainDay(int day); SkipToNextDay(); SpawnCow(); SpawnCustomer(); StartHunt(); ToggleHUD(); Update(); UpdateButtonNavigation(); UpdateText(); 

## class ClientPlayer : NetworkBehaviour
- fields: playerMan inventoryMan fpsScript teleportPlayer camHolder canvas weaponSway camShake 
- methods: CopyBackingFieldsToState(bool A_1); CopyStateToBackingFields(); Spawned(); 

## class ClydeDeathCutscene : NetworkBehaviour
- fields: clydeHolder clydeAnim player playerMan audioSequence audioIndex browseScript hittable cctvCam clydeText 
- methods: CloseText(); CopyBackingFieldsToState(bool A_1); CopyStateToBackingFields(); Die(); PlayDialogue(); PlayDialogue2(); PlayNextAudio(); PlayerBoredAnim(); PlayerShootAnim(); RelockMovement(); RevealText(); RevealText_(); Rpc_CMD_TriggerAnim(string trigger); Rpc_TriggerAnim(string trigger); Start(); StopBrowseScript(); TriggerAnim(string trigger); 

## class ColliderHighlighter : MonoBehaviour
- fields: solver 
- methods: Awake(); OnDisable(); OnEnable(); Solver_OnCollision(Il2CppSystem.Object sender, ObiNativeContactList e); 

## class CollisionEventHandler : MonoBehaviour
- fields: solver contactCount frame 
- methods: Awake(); OnDisable(); OnDrawGizmos(); OnEnable(); Solver_OnCollision(Il2CppSystem.Object sender, ObiNativeContactList e); 

## static class CommonUtils : Il2CppSystem.Object
- methods: GetRandomPrefabIndexes(int numRequired, ref Il2CppReferenceArray<GameObject> peoplePrefabs); 

## static class ComponentUtils : Il2CppSystem.Object

## class Computer : Interactable
- fields: dbIndex score normalizedNameLength searched inputFieldNavigation index newCam interacting tutorialScreen stopInteractEvent dialoguePickSFX otherComputerCanvas playerInput tabs selectedTabIndex k_shopTabIndex openingRecord computerScreen justAskedQuestion personWindow nameText idPhoto dobText statusText descButton descScrollRect descList emptyScrollView resultsScrollView noResultsScrollView emptyIdPhoto dBNames dBShowingNames resultButton searchField resultsHolder curDBName curShowingName resultsList npcAtCounter dialogueScript inputText aliveStatusButton closeRecordButton portraitButton dobButton 
- methods: ActuallySelectInputText(); Awake(); BestCandidateWindowScore(string search, string candidate); BuildNGrams(string text, int n); CheckDOB(); CheckDesc(int descIndex); CheckPicture(); CheckStatus(); CleanSearchText(string text); ClickResult(string index); CloseResult(); CopyBackingFieldsToState(bool A_1); CopyStateToBackingFields(); GetAdvancedSearchScore(string search, string candidate); HasCloseSubstring(string search, string candidateWord, int maxDistance); Interact(PlayerManager playerMan); LeaveComputerImmediate(); LevenshteinDistance(string a, string b); LoadDbNames(); NGramSimilarity(string a, string b, int n); NormalizeSearchString(string input); OnApplicationFocus(bool focus); OnControlsChanged(PlayerInput input); OnInputFieldSelected(); OnInteractPressed(InputAction.CallbackContext context); OnNextTabSelected(InputAction.CallbackContext context); OnPreviousTabSelected(InputAction.CallbackContext context); Rpc_CMD_ClickResult(string index); Rpc_CMD_CloseResult(); Rpc_CMD_SearchOnAllClients(string txt); Rpc_ClickResult(string index); Rpc_CloseResult(); Rpc_SearchOnAllClients(string txt); Rpc_TurnOnComputer(); Search(); SearchOnAllClients(); SelectInputTextIfNotAlreadySelected(); SelectTab(); SetIDValues(string name); SetNPCRecordUINavigation(); ShouldRankBefore(SearchMatch candidate, SearchMatch existing); Similarity(string a, string b); SplitWords(string input); StopInteract(); SubsequenceSimilarity(string a, string b); TurnOffComputer(); Update(); UpdateTabIndex(int index); Wishlist(); 

## class ConsoleUINavigation : MonoBehaviour
- fields: verticalUIElements 
- methods: AddElement(Selectable selectable); ContainsElement(Selectable selectable); OnEnable(); RefreshNavigation(); RemoveElement(Selectable selectable); 

## class ConsoleVideoMenu : MonoBehaviour
- fields: brightnessSlider flashingLightsToggle 
- methods: Start(); 

## class ConstrictedInteractable : Interactable
- fields: allowedItems constrictionAllows 
- methods: CheckForCurItem(int curIndex); CopyBackingFieldsToState(bool A_1); CopyStateToBackingFields(); Interact(PlayerManager playerMan); LookAt(); ReplaceCurItem(int item); Rpc_CMD_Interact(PlayerRef player); Rpc_Interact(PlayerRef player); Start(); TurnOffInteractable(); 

## class ControlSchemeSwitcher : MonoBehaviour
- fields: playerInput currentlyOnKeyboard 
- methods: Awake(); Start(); Update(); 

## class ControllerButtonPrompt : MonoBehaviour
- fields: changeTextToKeybindComponent 
- methods: OnDisable(); 

## class ControllerDisconnect : MonoBehaviour
- fields: playerInput uiElements previouslySelectedObj previousActionMap popupShowing 
- methods: Awake(); CloseDisconnectMessage(); OnDestroy(); OnInputDeviceChanged(InputDevice device, InputDeviceChange changeType); Update(); 

## class ControllerGlyphLookUpTable : ScriptableObject
- fields: ButtonID Glyph buttonID GlyphLookupTable 
- methods: FindGlyphForButtonID(string buttonID); 

## class ControllerLayoutMenu : MonoBehaviour
- fields: controlsMenuSwitcher startingObj controllerLayoutName currentLayoutIndex currentlyShownActionMap currentActionMap leftTriggerText leftBumperText rightTriggerText rightBumperText startText selectText leftStickText rightStickText buttonSouthText buttonNorthText buttonEastText buttonWestText dPadUpText dPadHorizontalText leftTriggerLine leftBumperLine rightTriggerLine rightBumperLine startLine selectLine leftStickLine rightStickLine buttonSouthLine buttonNorthLine buttonEastLine buttonWestLine dPadUpLine dPadHorizontalLine playerInput k_uitext k_uitext4 k_uitext3 k_uiRevChange k_moveKey k_jumpKey k_interactKey k_cameraKey k_crouchKey k_sprintKey k_useKey k_toggleDialogueHistoryKey k_backKey k_pushToTalkKey k_cycleInventoryKey k_dropKey k_secondaryInteractKey k_throwKey k_reloadKey k_rotateKey k_pauseKey k_cheatsKey k_scrubKey k_nextTabKey k_previousTabKey k_navigateKey k_scrollKey k_notFoundKey startingSensitivity startingFov startingBrightness startingInvertX startingInvertY startingFlashingLights startingCameraShake startingCameraBob sensitivitySlider invertXToggle invertYToggle fovSlider brightnessSlider flashingLightsToggle cameraShakeToggle cameraBobToggle actionMaps keybindsManager actionToLocalisedTextLookUp k_defaultSensitivity k_defaultInvertX k_defaultInvertY 
- methods: Apply(); Awake(); Back(); DisableTextElements(Il2CppReferenceArray<TextMeshProUGUI> textElements); OnEnable(); Reset(); SelectNextControllerLayout(); SelectPreviousControllerLayout(); SetControllerLayoutText(string actionMap); SetText(string actionName, Il2CppReferenceArray<TextMeshProUGUI> textElements); SetupKeysFromCurrentLanguage(); ToggleLines(Il2CppReferenceArray<GameObject> lines, bool active); 

## class ControlsMenuSwitcher : MonoBehaviour
- fields: keyboardControls gamepadControls playerInput showingKeyboard isShowing allowSwitchingInput backupEventSystemSelectedObj 
- methods: Awake(); HideControls(); ShowControlsBasedOnInput(); Update(); 

## class CoroutineRunner : MonoBehaviour
- fields: _instance 

## class CountingScript : MonoBehaviour
- fields: num text 
- methods: Update(); 

## class Cow : MonoBehaviour
- fields: target seeker pathfinder speed goingToPlayer matFader anim lookAt 
- methods: Disappear(); FixedUpdate(); GoToPlayer(); InitialDialogue(); LerpLookAtTarget(); Start(); 

## class CraneController : MonoBehaviour
- fields: cursor rope speed 
- methods: Start(); Update(); 

## class CreateLobbySettings : MonoBehaviour
- fields: lobbyType maxPlayers lobbyPassword createPasswordField lobbyNameField lobbyDropdown maxPlayersDropdown lobbySceneName fillAllFieldsWarning gamemode curNight alreadyGaveWarning maxPlayerWarning 
- methods: ChangeLobbyType(int type); ChangeMaxPlayers(int players); GetDefaultLobbyName(); MaxPlayerWarning(); OnEnable(); SetGamemode(int gamemode_); SetPassword(); StartLobby(); UpdateDropdownLanguage(); 

## class CreatingPoster : NetworkBehaviour
- fields: final posterStyle posterMesh posterStyles posterStyleIndex finalTexts tempText creatingCurrently cam pickupObj curPlayerMan saveSnapshotObj decorObject _pendingPosterString _initialized startingSelection playerInput completePosterSfx 
- methods: ActuallyUpdatePosterText(); CancelCreating(); ChangePosterStyle(bool forward); ConfirmPoster(); CopyBackingFieldsToState(bool A_1); CopyStateToBackingFields(); Creating(); Despawned(NetworkRunner runner, bool hasState); Rpc_CMD_ConfirmPoster(int posterStyle, string text); Rpc_ConfirmPoster(int posterStyle, string text); Rpc_LoadPoster(string savedString); SetInteractableTrue(); Spawned(); Start(); StartCreating(PlayerManager pMan); TryApplyPoster(); UpdatePosterText(); 

## class CurrentDayManager : NetworkBehaviour
- fields: storeMan customerGenManager eventMan listOfOccurrences todaysNpcs npcIndex curOccurrence occurrencesCompleted presetDayObjs randomOccurringDayObjs noDayObjsLeft day1Obj objsToTurnOffDay1 objsToTurnOffAfterDay1 truckHolder startedDay curDay objsToTurnOnBeforeCars objsToTurnOnAfterCars allCars gasPump alreadySetUpDay thieves thievesShuffled thievesShuffledIndex todaysDayObjNpc todaysDayObjCar eventNpcIndex eventNpcs notDay1Events computerDateText night4Dentist dentistOnBus dentistOnRoof dentistInStorage forestLimbs camouflagedMonster pets petSpawnPoints flickeringLightsAnim _waitingOnNetworkSpawn endlessMode anomalyLens forestRakes rakeIntroObjects rakeForeverObjects quotaNote dayOneEndlessSpawnPoint dayOneEndlessObjects allPets allFlesh alreadySpawnedPet canSpawnPetTonight doppelDrinkingPetrol tenAntlers creepyPainting bloodMoonSkybox regularSkybox alreadyCausedHunt weaponsArsenalIcon ambulance crawlingDentist bathroomGuy cow certainSpawnRakeIndex lemonadeStandPubert motorcyclePubert carPubert carsSpawnedToday pendingOccurrences processingQueue HalfSecondWait OneSecondWait 
- methods: ActuallyDoEvent(); ActuallyDoHunt(); ActuallySpawnNPC(); AddTrashToPetKnowledge(Trash trash); Awake(); CompleteOccurrence(); CopyBackingFieldsToState(bool A_1); CopyStateToBackingFields(); GetRandomCar(); HuntCaused(); PlayNextOccurence(); ProcessOccurrenceQueue(); Rpc_CMD_SpawnPet(int petType, string petName); Rpc_Day7(); Rpc_DisableQuotaNote(); Rpc_Enable10Antlers(); Rpc_EnableAmbulance(); Rpc_EnableAnomalyLens(); Rpc_EnableBathroomGuy(); Rpc_EnableCrawlingDentist(); Rpc_EnableCreepyPainting(); Rpc_EnableDentistInStorage(); Rpc_EnableDentistOnBus(); Rpc_EnableDentistOnRoof(); Rpc_EnableDoppelDrinkingPetrol(); Rpc_EnablePubert(int day); Rpc_NuisanceEvent(); Rpc_PurchaseTrapsHint(); Rpc_RakeGraffiti(); Rpc_RakeIntro(); Rpc_ScaryTruck(); Rpc_SetEndlessModeForClients(bool endlessMode_); Rpc_SpawnDentistNearDumpster(); Rpc_SpawnRake(int index); Rpc_SpawnTruck(); Rpc_TurnOffDay1Objs(); Rpc_TurnOffNotDay1Objs(); Rpc_TurnOnAfterCarObjs(); Rpc_TurnOnBeforeCarObjs(); Rpc_UpdateCurDayAndOccurrence(int day, int occurrence, bool endlessMode); SecondCheckForAfterCarObjs(); SetUpDay(); SpawnCar(); SpawnDayOneEndlessStuff(); SpawnPet(); SpawnRake(); SpawnThief(); Spawned(); Start(); 

## class CursorControlSchemeGate : MonoBehaviour
- fields: playerInput cursorUnlockRequested 
- methods: ApplyCursorState(); Awake(); OnControlsChanged(PlayerInput changedPlayerInput); OnDisable(); OnEnable(); RequestCursorLock(); RequestCursorUnlock(); SetCursorUnlockRequested(bool shouldUnlock); 

## class CursorController : MonoBehaviour
- fields: minLength speed cursor rope 
- methods: OnEnable(); Update(); 

## class CustomerEmotionHandler : MonoBehaviour
- methods: Start(); 

## class CustomerKillingEntity : NetworkBehaviour
- fields: seeker pathfinder moveSpeed reachedDistance retargetInterval playerAttackDistance attackMoveSpeed damageToPlayer attackDamageDelay attackDuration anim runAnim attackAnim matFaders exitReachedDistance fadeDuration runningToExit startedFadingAway exitCoroutine target targetPlayer currentAttackTarget targetingPlayer attacking hasReachedNearestNPC nextRetargetTime hasSnappedToInitialPosition takeDamageSFX screamSFX damageToNPC _NetworkedPosition _NetworkedRotation _IsPositionInitialised 
- methods: AttackPlayer(); Awake(); BeginRunToExitAndFade(); ClearTarget(); CopyBackingFieldsToState(bool A_1); CopyStateToBackingFields(); CurrentTargetIsInvalid(); DealDamageToNPC(); DealDamageToPlayer(); DisableProxyNavigation(); EnableNavigation(); FindBestTarget(); FindNearestNPC(); FindNearestPlayer(); FinishAttack(); FinishNPCAttack(); FixedUpdateNetwork(); GoToExitAndFade(); ReachNPC(); ReachedNearestNPC(); Render(); Rpc_CMD_RunToExitAndFade(); Rpc_StartExitFade(float duration); Rpc_TriggerAnimation(string triggerName); RunToExitAndFade(); SetTarget(Transform newTarget, bool isPlayer, PlayerManager playerManager); Spawned(); TriggerAnimation(string triggerName); 

## class CustomizablesLocker : MonoBehaviour
- fields: slotAnimators curSelection title equipButton equipped thirdPersonMan scrollRect 
- methods: Equip(); OnEnable(); Select(int index); 

## class DEMO_LegsAnim_LASwitcher : MonoBehaviour
- fields: Switch 
- methods: SwitchLegsAnimator(); 

## class DEMO_LegsAnim_RedirectExampleJoy : MonoBehaviour
- fields: Legs Joystick DebugWSAD ConstantDebugInputVal ModuleBlend module 
- methods: Start(); Update(); UpdateInputs(); 

## class DEMO_LegsAnim_RedirectVector : MonoBehaviour
- fields: Legs Dir 
- methods: Start(); Update(); 

## class DEMO_LegsAnim_StepEvents : MonoBehaviour
- fields: StepSource StepClips LandClips Particle 
- methods: LegAnimatorStepEvent(LegsAnimator.Leg leg, float power, bool isRight, Vector3 position, Quaternion rotation, LegsAnimator.EStepType type); PlayLandAudio(float volumeMul = 1f); PlayStepAudio(float volumeMul = 1f); 

## class DEMO_LegsAnim_TriggerImpact : MonoBehaviour
- fields: TriggerOn Landing Stopping GetHit 
- methods: CallGetHitImpact(); CallImpact(LegsAnimator.PelvisImpulseSettings settings); CallLandingImpact(); CallStoppingImpact(); 

## class DamageTrigger : MonoBehaviour
- fields: damageOnTrigger triggerEvent 

## class DayObjectManager : MonoBehaviour
- fields: spawnsNpc npcToSpawn spawnsCar carToSpawn minIndexToSpawn maxIndexToSpawn 

## class DebugParticleFrames : MonoBehaviour
- fields: actor size 
- methods: Awake(); OnDrawGizmos(); 

## class DecorObject : MonoBehaviour
- fields: decorPoints decorParticlesPoint autoIncreaseParticles decorIncreaseParticles decorDecreaseParticles 
- methods: DecreaseParticles(); IncreaseParticles(); OnEnable(); 

## class DelayedEventTrigger : MonoBehaviour
- fields: animEvent spawnCoinPositions coin 
- methods: ExecuteEvent(); NPCsRunAway(); NPCsWalkAway(); OnEnable(); PauseAmbience(); SpawnCoins(); 

## class Delete : MonoBehaviour
- fields: turnOffAfterTime destroy time randomTime 
- methods: OnEnable(); TurnOff(); 

## class DemoManager : NetworkBehaviour
- methods: CopyBackingFieldsToState(bool A_1); CopyStateToBackingFields(); 

## class DialogueInteractable : Interactable
- fields: dialogueId nonCustomerDialogueInteractable player rotationSpeed angleThreshold head initialInteraction initialInteractionSpeakTime dialogueOptionsCanvas dialogueOptionsContent pathfindScript cantExitDialogue mouthAnim askedQuestion playerName forceDialogue replaceInitialDialogueWithForced dontDoInitialDialogue timesToNotExitDialogue interacting faceNearestPlayerAfterTalking faceNearestPlayer takesNameOfRandomPlayer nameTag dialoguePickSFX shirt shirtColors inQuestioningMenu cantBeAskedAboutMood setNameToRandomSteamUser cantEscapeToExit canOnlyInteractOnce timeBeforeInteractionShowsUp questionAfterInitialInteraction questionCanvas questionCanvasSelectedObj playerInput k_headButton k_clothesButton k_limbsButton k_movementButton k_exitButton cantQuestionFromComputer hasCompletedTransaction askedCanvasQuestion firstTimeExitDialogue target onlyLookAtWithHead headPivot dontRotateBody 
- methods: ActuallyPersonallyExitDialogue(); AskQuestion(string question); Awake(); CanPauseAgain(); CantPause(); CleanupBeforeDestroy(); CompleteTransaction(); CopyBackingFieldsToState(bool A_1); CopyStateToBackingFields(); DisableInteract(); EnterDialogue(); ExitDialogue(); ExitInteraction(); ForceTalkToPlayer(); GetRandomSteamUsernameFromLobby(); InitialDialogue(bool onlyClientSide); InitialDialogueClientSide(); Interact(PlayerManager playerMan); LerpLookAtTarget(); OnDialogueSelected(InputAction.CallbackContext context); Rpc_AskedCanvasQuestion(); Rpc_CMD_AskedCanvasQuestion(); Rpc_CMD_SetColor(int colorIndex); Rpc_CMD_StartLerpLookAtTarget(Vector3 target); Rpc_SetColor(int colorIndex); Rpc_SetName(string name); Rpc_StartLerpLookAtTarget(Vector3 target_); SetColor(); SetTargetToNearestPlayer(); Start(); StopMouth(); TurnOnDialogueOptions(); Update(); 

## class DialogueMenu : MonoBehaviour
- fields: selectObj targets curSelection onItemSelect switchSFX pickSFX exitSFX playerInput previousActionMap 
- methods: Awake(); ConvertStringToKeyCode(string keyName); OnEnable(); Update(); 

## class DialogueTutorialManager : NetworkBehaviour
- fields: alreadyDone scannedID questionedOccupation questionedAppearance tickedCheckbox scannedID_ questionedOccupation_ questionedAppearance_ canvas alreadyToldAboutGun amountOfObjectivesDone amountCompleted pointAtObjectiveArrow finalCheckbox _spawned 
- methods: Awake(); CheckIfAllObjectivesDone(); CompletedTransaction(); CopyBackingFieldsToState(bool A_1); CopyStateToBackingFields(); Despawned(NetworkRunner runner, bool hasState); GunUnlocked(); QuestionedAppearance(); QuestionedOccupation(); Rpc_CMD_QuestionedAppearance(); Rpc_CMD_QuestionedOccupation(); Rpc_CMD_ScannedID(); Rpc_QuestionedAppearance(); Rpc_QuestionedOccupation(); Rpc_ScannedID(); ScannedID(); Spawned(); 

## class DisableAfterNetworkRegister : NetworkBehaviour
- methods: CopyBackingFieldsToState(bool A_1); CopyStateToBackingFields(); 

## class DisableInKBM : MonoBehaviour

## class DisableOnDisable : MonoBehaviour
- methods: OnDisable(); 

## class DisableTracer : MonoBehaviour
- fields: target lastEnabled 
- methods: Awake(); LateUpdate(); 

## class DissonanceInputModeDropdown : MonoBehaviour
- fields: dropdown voiceConnection PrefKey _currentMode micScript _recorder 
- methods: ApplyMode(UiMode mode, bool save); Awake(); OnDestroy(); OnDropdownChanged(int index); SetRecorder(bool voiceDetection, bool transmit); Start(); UpdateAllLanguageChanges(); 

## class DistanceFade2DAudio : MonoBehaviour
- fields: minDistance maxDistance maxVolume fadeSpeed updateRate audioSource listenerTransform targetVolume nextUpdateTime 
- methods: Awake(); GetVolumeFromDistance(float distance); RefreshListenerFromCameraManager(); Update(); UpdateTargetVolume(); 

## class DitherFader : MonoBehaviour
- fields: duration duration _material Transparency startFadeInSpeed 
- methods: Awake(); FadeIn(float duration); FadeOut(float duration); Start(); 

## class DontDelete : MonoBehaviour
- methods: Start(); 

## class DontKeepOnNewDay : MonoBehaviour
- methods: Start(); Update(); 

## class DragItem : MonoBehaviour
- fields: itemIndex col 

## class DropdownFocusOnOpen : MonoBehaviour
- fields: dropdown hasFocus _scrollRectControllerFocus 
- methods: Awake(); OnDisable(); Update(); 

## class Dumpster : ConstrictedInteractable
- methods: CopyBackingFieldsToState(bool A_1); CopyStateToBackingFields(); Interact(PlayerManager playerMan); 

## class DumpsterMonster : Hittable
- fields: monster anim positions curPosition insideDumpsterMonster doneInsideBit hitCollider alreadyHit 
- methods: Awake(); CopyBackingFieldsToState(bool A_1); CopyStateToBackingFields(); Hit(float damage, Vector3 hitFrom, bool alwaysTriggerDamageReaction = false, bool tempStun = true, string damageType = "Normal"); Rpc_CMD_RunToSide(); Rpc_CMD_SetPosition(int pos); Rpc_RunToSide(); Rpc_SetPosition(int pos); SetPosition(int pos); 

## class DynamicObstacle : MonoBehaviour
- methods: Start(); 

## class EODEvent : MonoBehaviour
- fields: curEvent eventsOverTime timeBetweenEvents 
- methods: AutoEnableNextObject(); Start(); 

## class EODReportValues : MonoBehaviour
- fields: mandatoryRevenue todayMoneyGained todayMoneyLost doppelsLetThru npcID npcKilledID todaysDayObjIndex curDay personalFunds eventsToBeQueued humanKilledName endlessMode petDying CarryOverPersonalFunds 
- methods: Awake(); ClearEventsToBeQueued(); ResetPersonalFunds(); Start(); 

## class Egg : MonoBehaviour
- fields: eggDiffuser curGrabItemTooltip grabItemTooltip 
- methods: FixedUpdate(); OnDisable(); OnTriggerEnter(Collider other); Start(); 

## class EggDiffuser : NetworkBehaviour
- fields: funnelCover outline pulverizeVfx pulverizeSfx dropTooltip purchaseTooltip 
- methods: CopyBackingFieldsToState(bool A_1); CopyStateToBackingFields(); Egg(); PurchaseHint(); Rpc_CMD_Egg(); Rpc_Egg(); TryTurnOnOutline(); TurnOffOutline(); 

## class EmailManager : NetworkBehaviour
- fields: inboxEmailsByDay viewEmailsByDay newEmailNotif curDay 
- methods: Awake(); ClickEmail(int dayEmailIndex); CopyBackingFieldsToState(bool A_1); CopyStateToBackingFields(); Rpc_CMD_CheckMail(); Rpc_CheckMail(); 

## class EndMenu : MonoBehaviour
- fields: started cam camCenter busAudio 
- methods: LoadGame(); Review(); StartGame(); Update(); Wishlist(); 

## class EndOfDayReport : NetworkBehaviour
- fields: demo showingRevenue actualRevenue todaysQuotaText totalRevenueText quotaReached quotaMissed nextDayButton restartDayButton finishGameButton alreadyCompleted amountOfCompletions amountToLookAtText fadeOut eodValues moneyAudioArray npcFolders shownQuotaGoal shownTickDown npcIcons npcKilledID todayMoneyLost showingMoneyLoss tickRevenue tickDownRevenue noTickDownObj tickDownObj tickDownText nightCompleteText playerInput customerDataNotSaved personalEarnings storePercentageEarningsText totalPersonalEarningsText todayPersonalEarnings storyModeManager eventsObj eodReportHolder endlessModeNextButton petImage playerCount personalStatsHolder hitQuota noStorePercentageEarnings curCharacterIndex youDiedFolder customerReportScrollHolder singleplayerWordedObjs multiplayerWordedObjs 
- methods: Awake(); BackToGameScene(); ChangePetSprite(int emotion); CheckWhosCompletedDay(); CompleteDay(); CopyBackingFieldsToState(bool A_1); CopyStateToBackingFields(); DoStartThings(); FinishGame(); FixedUpdate(); LoadEndScene(); LockCursor(); OnSelectPressed(InputAction.CallbackContext context); ResetAllValues(); Rpc_AnotherPlayerCompleted(); Rpc_CMD_AnotherPlayerCompleted(); Rpc_CMD_ConfirmWeAreInMultiplayer(); Rpc_CMD_EveryoneConfirmNext(); Rpc_CMD_LoadEndScene(); Rpc_CheckTickDown(); Rpc_EnableFinishGameBTN(); Rpc_EnableNextDayBTN(); Rpc_EnableRestartDayBTN(); Rpc_EveryoneConfirmNext(); Rpc_LoadEndScene(); Rpc_SetToMultiplayerWordingRpc(); Rpc_ShowNextButton(); Rpc_ShowNextCharacter(int id); Rpc_ShowPersonalEarnings(); Rpc_ShowQuota(bool hitQuota_); Rpc_StartCustomerReport(); Rpc_TickDownRevenue(); Rpc_TickRevenue(); Rpc_UpdatePlayerCount(int count); Rpc_UpdateVariables(float revenue, float moneyLost, Il2CppStructArray<int> killedID, float quota); ServerLoadEndScene(); SetPetHappySprite(); ShowNextCharacter(); ShowQuota(); Spawned(); Start(); StartTickingRevenue(); UnlockCursor(); UpdatePersonalEarnings(); 

## class EndingScene : NetworkBehaviour
- fields: finalTallyMark resetSaveOnStart switchToEndlessModeOnStart petAliveEnding gainAchievementOnStart achievementGained endingUnlocksEndlessMode voiceManagerObj 
- methods: ActuallyGoBackToGame(); Awake(); CopyBackingFieldsToState(bool A_1); CopyStateToBackingFields(); DestroyPlayers(); EnableTally(); GoBackToGame(); GoBackToMenu(); ResetSave(); Rpc_ActuallyGoBackToMenu(); SpawnVoiceManager(); Spawned(); UnlockCursor(); 

## class EndlessGenerationManager : MonoBehaviour
- fields: curDay killed spawnedBefore mandatoryIds entertainmentRank allRandomSpawningNpcs endlessGenSpawningNpcs demoRandomSpawningNpcs curDifficulty 
- methods: GenerateNight(); Method_Internal_Static_Boolean_IEnumerable_1_String_IEnumerable_1_String_0(IEnumerable<string> source, IEnumerable<string> values); Method_Internal_Static_Boolean_IEnumerable_1_String_IEnumerable_1_String_1(IEnumerable<string> source, IEnumerable<string> required); Method_Internal_Static_String_String_0(string id); 

## class Enemy : NetworkBehaviour
- fields: leaveLocation leaving speedMultiplier hittable 
- methods: ChaseNonPlayerTarget(Vector3 targPosition); CheckIfNearBarricade(); CopyBackingFieldsToState(bool A_1); CopyStateToBackingFields(); Leave(); 

## class EnemyHolder : MonoBehaviour
- fields: enemy 

## class EnterOnlyEndEdit : MonoBehaviour
- fields: input pressEnterEvent playerInput lastInvokedText hasInvokedEvent 
- methods: Awake(); OnDestroy(); OnEndEdit(string text); 

## class EntryDoor : NetworkBehaviour
- fields: sfx anim doNoise monsterCheckPosition canEnter causesMonstersToCheck 
- methods: AllowEnter(); CopyBackingFieldsToState(bool A_1); CopyStateToBackingFields(); DoNoise(); GetXZDistance(Transform a, Transform b); MonstersCanCheck(); Rpc_ActuallyEnter(); Rpc_CMD_Enter(); 

## class EpilepsyHandler : MonoBehaviour
- methods: OnEnable(); 

## static class EventConsts : Il2CppSystem.Object
- fields: MAX_EVENT_VALUE LOSE_GAME RATS_KILLED_OBJECTIVE ROACHES_KILLED_OBJECTIVE BOUNTY_CLAIMED DENTIST_HAS_RETURNED CALENDAR_KILLER_STRIKE_TOMORROW CALENDAR_KILLER_DID_NOT_STRIKE SURGERY_BIWEEKLY_PAYMENT SURGERY_3_DAYS_UNTIL SURGERY_2_DAYS_UNTIL SURGERY_1_DAYS_UNTIL SURGERY_PAYMENT_1_DAYS_REMAINING SURGERY_PAYMENT_2_DAYS_REMAINING SURGERY_LAST_CHANCE_TO_PAY COME_TOMORROW_MAIL SURGERY_SUCCESS SURGERY_FAILED CLYDE_DAWSON_KILLED_THIS_SHIFT CLYDE_DAWSON_KILLED CLYDE_DAWSON_CALL ARRIVE_AT_DOCTOR 

## class EventIfLookTowards : NetworkBehaviour
- fields: isRpc eventWhenLookedAt alreadyLookedAt 
- methods: CopyBackingFieldsToState(bool A_1); CopyStateToBackingFields(); EventWhenLookedAt(); Rpc_CMD_EventWhenLookedAt(); Rpc_EventWhenLookedAt(); 

## class EventManager : NetworkBehaviour
- fields: eventAtlas targetEventIndex phoneRingingSfx eodBus eodFakeBus startPos playerDoppelganger playerDoppelgangerSpawnpoint playerDoppelgangerSpawnpoints flickeringLightsEvent truckHolder eightPlayerDoppels alreadyAggro multiplePlayerDoppelgangers vanessa vanessaDeadOnCounter bloodParticles horrorSfx rat roach 
- methods: AggroAllOtherEmployeeCopies(); Awake(); CopyBackingFieldsToState(bool A_1); CopyStateToBackingFields(); NoEventOccursTonight(); Rpc_BloodMoonExplanation(); Rpc_CallOnCorpseNightRpc(); Rpc_DisableEODBus(); Rpc_EODBus(); Rpc_EODFakeBus(); Rpc_FlickeringLightsEvent(); Rpc_SpawnChasingNathan(); Rpc_SpawnForestLimbs(); Rpc_SpawnNight1TruckRpc(); Rpc_SpawnRatInfestation(); Rpc_SpawnRoachInfestation(); Rpc_SpawnTruck(); Rpc_StartShrineEvent(); Rpc_SuspiciousCustomerRpc(); Rpc_VanessaDeadEvent(StoreBrowseBehaviour npc); Spawn8PlayerDoppelgangers(); SpawnPlayerDoppelganger(); SpawnRat(); SpawnRats(int amount); SpawnRoach(); VanessaDeadEvent1(); VanessaDeadEvent2(); 

## class EventOnEndless : MonoBehaviour
- methods: Start(); Update(); 

## class EventOnThisObjDisable : MonoBehaviour
- fields: selfDisableEvent 
- methods: OnDisable(); 

## class EventSystemChecker : MonoBehaviour
- methods: Update(); 

## class Explosion : NetworkBehaviour
- fields: playExplosionOnAwake explosionRadius enemyCloseDamage enemyFarDamage playerCloseDamage playerFarDamage camShakeIntensity explosionParticles 
- methods: CopyBackingFieldsToState(bool A_1); CopyStateToBackingFields(); PlayExplosion(); Spawned(); 

## class ExtendableGrapplingHook : MonoBehaviour
- fields: solver character material section hookResolution hookExtendRetractSpeed hookShootSpeed particlePoolSize rope blueprint ropeRenderer cursor hookAttachment 
- methods: AttachHook(); Awake(); DetachHook(); LaunchHook(); LayParticlesInStraightLine(Vector3 origin, Vector3 direction); OnDestroy(); Update(); 

## class ExtrapolationCamera : MonoBehaviour
- fields: target extrapolation smoothness linearSpeed rotationalSpeed distanceFromTarget lastPosition extrapolatedPos 
- methods: FixedUpdate(); LateUpdate(); Start(); Teleport(Vector3 position, Quaternion rotation); 

## class FHierarchyIcons : Il2CppSystem.Object
- methods: DrawIcon(string texName, Rect rect); EvaluateIcons(int instanceId, Rect selectionRect); GetTex(string name); 

## class FPD_FixedCurveWindowAttribute : PropertyAttribute
- fields: StartTime EndTime StartValue EndValue Color 

## class FPD_HeaderAttribute : PropertyAttribute
- fields: HeaderText UpperPadding BottomPadding Height 

## class FPD_LayersAttribute : PropertyAttribute

## class FPD_PropertiesFoldoutAttribute : PropertyAttribute
- fields: HowManyNextPropertiesToContain foldout title indent frameStyleID frameStyle extraSpacing 

## class FPD_ResourcesIconAttribute : PropertyAttribute
- fields: Path Spacing 

## class FPD_SingleLineTwoPropsAttribute : PropertyAttribute
- fields: PropName LabelWidth SecLabelWidth MiddlePadding UpPadding AddSecondPropWidth 

## class FPD_SuffixAttribute : PropertyAttribute
- fields: Min Max Mode Suffix editableValue widerField 

## class FPSController : NetworkBehaviour
- fields: time position rotation playerCamera k_MovementVolumeBoost crouchSpeed walkSpeed runSpeed downedSpeed gravity jumpPower globalSensitivityMultiplier lookXLimit verticalVelocity localPitch lockMove lockCam lookAtState headbobAnim focusFOV runParticles characterController playerMan interactMan inventoryMan objectToLookAt justGrounded wasInLookAtState justLockedCam justUnlockedCam jumpSFX landSFX characterAnim volume lockVolume canRun thirdPersonMan _SpawnPosition _SpawnRotation _IsMoving _IsRunning _IsCrouching _IsDowned _IsGroundedNet _NetPosition _NetRotation _IsLockedMoveNet _IsLookAtStateNet _lastSyncedPosition _lastSyncedRotation _lastSyncedIsMoving _lastSyncedIsRunning _lastSyncedIsCrouching _lastSyncedIsDowned k_PositionThreshold k_RotationThreshold _lastSyncedLockMove _lastSyncedLookAtState headCheck headHitLayer _localIsCrouching wasCrouching lookAtSpeed sensitivityMultiplier moveMultiplier canSprint justLockedMove playerInput k_Sensitivity k_InvertX k_InvertY isSprinting fovAdjustment forcedToSprint localYaw grounded timeSinceLanded syncTimer k_SyncInterval prevGrounded k_SnapshotBufferSize k_InterpolationDelay _snapshots _snapshotCount _snapshotHead crouchToggleEnabled crouchToggled 
- methods: AddSnapshot(Vector3 position, Quaternion rotation); ApplyGravityOnly(); ChangeVolume(float volume_); ConvertStringToKeyCode(string keyName); CopyBackingFieldsToState(bool A_1); CopyStateToBackingFields(); FixedUpdateNetwork(); ForceLookRotation(Quaternion rot); GetDistanceToFOVLinearGraph(float x); HandleProxyAnims(); InterpolateProxy(); IsSprintPhysicallyHeld(InputAction sprintAction); LockCursor(); LookAtState(); Move(bool usingGamepad); OnGamePaused(); Render(); Rpc_BroadcastGrounded(bool isGrounded); Rpc_BroadcastSnapshot(Vector3 position, Quaternion rotation); Rpc_CMD_ChangeVolume(float v); Rpc_CMD_SyncGrounded(bool isGrounded); Rpc_CMD_SyncToServer(Vector3 position, Quaternion rotation, bool isMoving, bool isRunning, bool isCrouching, bool isDowned, bool lockMove_, bool lookAtState_); Rpc_ChangeVolume(float volume_); Spawned(); Start(); TryUncrouch(); UnlockCursor(); UnlockVolume(); Update(); UpdateLocalVisuals(); 

## class FPSDisplay : MonoBehaviour
- fields: updateInterval showMedian medianLearnrate accum frames timeleft currentFPS median average uguiText 
- methods: ResetMedianAndAverage(); Start(); Update(); 

## static class FSceneIcons : Il2CppSystem.Object
- methods: SetGizmoIconEnabled(Il2CppSystem.Type type, bool on); SetGizmoIconEnabled(MonoBehaviour beh, bool on); 

## class FadeExample : MonoBehaviour
- fields: _material _reappearing Transparency fadeSpeed 
- methods: Awake(); Update(); 

## class FakeBusEvent : NetworkBehaviour
- fields: realBus getToRealBusCanvas chasingCreature1 chasingCreature2 spawnChaseCreature1 spawnChaseCreature2 openDoor closedDoor stillDrivingCreature alreadyStartedChase 
- methods: Awake(); CopyBackingFieldsToState(bool A_1); CopyStateToBackingFields(); Rpc_StartChase(); 

## class FinalSequenceManager : NetworkBehaviour
- fields: finalSequenceSpawnPoints inStorePosition inStoreRotation resetSequenceEvent resetAnimations finaleOffice 
- methods: Awake(); CancelDisablePauseDuringFinaleChase(); CopyBackingFieldsToState(bool A_1); CopyStateToBackingFields(); FixedUpdate(); ReDisableResetAnimations(); ResetFinalSequence(); ReturnToStore(); Start(); StartSequence(); StartedFinalCutscene(); TeleportToStore(); UnpauseInvoked(); 

## class FinaleStartCutscene : MonoBehaviour
- fields: cam camStartPoint cutsceneAnim startedCutscene reenableHuntLight disablePauseDuringFinaleChase 
- methods: FinishCutscene(); OnEnable(); SmallBlink(); StartActualCutscene(); Update(); 

## class FindLayerInPrefabs : Il2CppSystem.Object

## class FlashlightVisible : NetworkBehaviour
- fields: visible invisible inspectable 
- methods: CopyBackingFieldsToState(bool A_1); CopyStateToBackingFields(); FlashlightDisabled(); FlashlightEnabled(); Rpc_CMD_FlashlightDisabled(); Rpc_CMD_FlashlightEnabled(); Rpc_FlashlightDisabled(); Rpc_FlashlightEnabled(); 

## class FlickeringLightsEvent : NetworkBehaviour
- fields: genSwitch spawnCopyPos employeeCopy genSmokeParticles storeLights entryDoor finished 
- methods: CopyBackingFieldsToState(bool A_1); CopyStateToBackingFields(); FlickSwitch(); Rpc_CMD_FlickSwitch(); Rpc_FlickSwitch(); 

## class ForbiddenWords : MonoBehaviour
- fields: forbiddenWords 
- methods: CensorText(string text); 

## class ForestLimbsEvent : MonoBehaviour
- fields: limbs limbsRemaining limbsObjective existingFence done forestObjects 
- methods: CheckHowManyLimbsLeft(); DisableLimbsObjective(); OnEnable(); Start(); StartEvent(); 

## class FusionCallbackBase : NetworkBehaviour
- methods: CopyBackingFieldsToState(bool A_1); CopyStateToBackingFields(); Despawned(NetworkRunner runner, bool hasState); OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason); OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, Il2CppStructArray<byte> token); OnConnectedToServer(NetworkRunner runner); OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, Il2CppSystem.Object> data); OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason); OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken); OnInput(NetworkRunner runner, NetworkInput input); OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input); OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player); OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player); OnPlayerJoined(NetworkRunner runner, PlayerRef player); OnPlayerLeft(NetworkRunner runner, PlayerRef player); OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress); OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, Il2CppSystem.ArraySegment<byte> data); OnSceneLoadDone(NetworkRunner runner); OnSceneLoadStart(NetworkRunner runner); OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList); OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason); OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message); Spawned(); 

## class FusionNetworkManager : MonoBehaviour
- fields: excludePlayer ticks gameObject player runner platformID muted sessionName region sessionName mode region sessionName ENABLE_DEBUG_FUSION _menuScene _gameScene _EODReportScene _endMenuScene _splashScene _loseGameScene _ending1Scene _ending2Scene _ending3Scene _ending4Scene _runner _sceneManager playerPrefab_Lobby playerPrefab_Game _spawnedPlayers _isShuttingDown _sceneReadyForSpawn _pendingSpawns _isRetrying _networkLostExternally _speakerPrefab _playerVoiceMixerGroup _voiceRecorder _actorToFuid _pendingPermissionCheckActors _desiredMuteStateByXuid OnVoiceConnectionReady _startSpawnPoint _spawnIter _abortStartGame _localCommsBlanketMute 
- methods: AbortStartGame(); ApplyMuteToActor(int actorNumber, bool muted); Awake(); CleanUpRunner(); ClearNetworkLostExternally(); DestroyAfterTicks(GameObject gameObject, int ticks); DestroyItem(GameObject gameObject); FusionFullLog(NetworkBehaviour netBehaviour, string text); FusionLog(string text); GetActivePlayers(); GetMappedFuids(); GetPlayer(PlayerRef player); GetPlayerConnectedCount(PlayerRef excludePlayer = default(PlayerRef); GetRunner(); IsSoloMode(); LeaveGame(); MoveToScene(SceneRef nextScene); OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason); OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, Il2CppStructArray<byte> token); OnConnectedToServer(NetworkRunner runner); OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, Il2CppSystem.Object> data); OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason); OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken); OnInput(NetworkRunner runner, NetworkInput input); OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input); OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player); OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player); OnPlayerJoined(NetworkRunner runner, PlayerRef player); OnPlayerLeft(NetworkRunner runner, PlayerRef player); OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress); OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, Il2CppSystem.ArraySegment<byte> data); OnSceneLoadDone(NetworkRunner runner); OnSceneLoadStart(NetworkRunner runner); OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList); OnShutdown(NetworkRunner runner, ShutdownReason reason); OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message); RetryPendingVoicePermissionChecks(); Rpc_RequestDestroyItem(NetworkObject netObject); Rpc_RequestSpawnItem(NetworkObject prefab, Vector3 position, Quaternion rotation, Il2CppSystem.Action<NetworkObject> postSpawn); SetAllRemoteVoiceMuted(bool muted); SetNetworkLostExternally(); SetRemotePlayerVoiceMuted(string platformID, bool muted); SetVoiceMuted(bool muted); SetupRunner(GameObject runnerGO); ShutdownSession(); SpawnItem(NetworkObject prefab, Vector3 position, Quaternion rotation, Il2CppSystem.Action<NetworkObject> postSpawn = null); SpawnPlayerInGameScene(NetworkRunner runner, PlayerRef player, int index); StartAsClient(string sessionName, string region = ""); StartAsHost(string sessionName); StartAsSolo(); StartGame(GameMode mode, string sessionName, string region = ""); Update(); ValidateSceneNetworkObjects(); add_OnVoiceConnectionReady(Il2CppSystem.Action<VoiceConnection> value); remove_OnVoiceConnectionReady(Il2CppSystem.Action<VoiceConnection> value); 

## class GameManager : NetworkBehaviour
- fields: Instance 
- methods: Awake(); CheckAllReady(); CopyBackingFieldsToState(bool A_1); CopyStateToBackingFields(); 

## class GamerTagsScreen : MonoBehaviour
- fields: gamertagPrefab scrollRectContent closeButton shownGamerTags 
- methods: OnEnable(); 

## class GasCollisionSpawner : NetworkBehaviour
- fields: objectToSpawn ps spawnEveryXCollisions curSpawnEveryXCollisions timeBeforeReset curTimeBeforeReset onlySpawnParticleIfServer 
- methods: CopyBackingFieldsToState(bool A_1); CopyStateToBackingFields(); FixedUpdate(); KillClosestParticleAt(Vector3 hitPos, Il2CppStructArray<ParticleSystem.Particle> particles, int alive, float threshold = 0.1f); KillCollidedParticles(GameObject other); OnParticleCollision(GameObject other); Rpc_CMD_SpawnParticle(Vector3 pos, Quaternion rot); Rpc_SpawnParticle(Vector3 pos, Quaternion rot); SpawnParticle(Vector3 pos, Quaternion rot); Start(); 

## class GasPumpHoses : NetworkBehaviour
- fields: positionToFollow target hosePump upPoint index 
- methods: Awake(); ChangeRopeBulge(Vector3 _positionToFollow, bool bulgeOn); ConnectRope(Vector3 _positionToFollow); CopyBackingFieldsToState(bool A_1); CopyStateToBackingFields(); DisconnectRope(Vector3 _positionToFollow); FixedUpdate(); Rpc_CMD_ChangeRopeBulge(Vector3 _positionToFollow, bool bulgeOn); Rpc_CMD_ConnectRope(Vector3 _positionToFollow); Rpc_CMD_DisconnectRope(Vector3 _positionToFollow); Rpc_ChangeRopeBulge(Vector3 _positionToFollow, bool bulgeOn); Rpc_ConnectRope(Vector3 _positionToFollow); Rpc_DisconnectRope(Vector3 _positionToFollow); 

## static class GdkPlatformSettings : Il2CppSystem.Object
- fields: gameConfigTitleId gameConfigScid gameConfigSandbox 

## class GeneratorSwitch : Interactable
- fields: powerOff 
- methods: CopyBackingFieldsToState(bool A_1); CopyStateToBackingFields(); Interact(PlayerManager playerMan); Rpc_Interact(PlayerRef player); 

## static class GlobalUINavigation : Il2CppSystem.Object
- fields: eventSystem sendNavigationEvents selectedObject inputModule move submit cancel savedNavigation savedEventSystems selectableBuffer watcher navigationLockCount lastSelectableCount 
- methods: ApplyDisabledState(EventSystemState state); DisableAllActiveSelectables(); DisableCurrentEventSystem(); DisableNavigation(); EnableNavigation(); EnsureBufferCapacity(int requiredCapacity); EnsureWatcher(); ForceEnableNavigation(); IsInputFieldSelected(EventSystem eventSystem); RefreshNavigationLock(); ResetStatics(); RestoreEventSystems(); RestoreSelectableNavigation(); 

## class GlyphManager : MonoBehaviour
- fields: controller lookupTable Glyphs keybindToButtonID playerInput xboxGlyphs 
- methods: Awake(); GetGlyphKeyForKeybind(int keybind); OnControlsChanged(PlayerInput input); OnDestroy(); 

## class GrapplingHook : MonoBehaviour
- fields: solver character hookExtendRetractSpeed material section rope blueprint ropeRenderer cursor hookAttachment 
- methods: AttachHook(); Awake(); DetachHook(); LaunchHook(); OnDestroy(); Update(); 

## class GunCase : Interactable
- fields: isActuallyGunCase destroyObjectTag objIndex allowEveryoneToHave canPickupItem 
- methods: CopyBackingFieldsToState(bool A_1); CopyStateToBackingFields(); Interact(PlayerManager playerMan); Rpc_Interact(PlayerRef playerRef); 

## class HatsRenderer : MonoBehaviour
- fields: hatObjs 
- methods: Awake(); SelectHat(int index); 

## class HideChildrenOnDisable : MonoBehaviour
- fields: children 
- methods: OnDisable(); 

## class Hittable : NetworkBehaviour
- fields: delay obj normalDamageMultiplier fireDamageMultiplier bludgeonDamageMultiplier fireObj stunnedObj isEntity health maxHealth deathEvent hitEvent deathObj hitObj killingCancelsTransaction browseScript browseNPCScript nuisanceNPCScript dialogueScript col fleshSpawns fleshSpawnPoints headMaterial bodyMaterial path cancelSpawnNextNpc die enemy chaseScript ChasePlayerAfterHit hittingCausesFinalSequence gameDone dontTurnToPlayer dontDestroyOnDeath makeEventOccurAtXHealth xHealth eventAtXHealth affectsTimeRemaining dontPunishForKilling returnMoneyIfKilledWhenLeaving returnMoneyIfKilledAnyway moneyToReturn returnMoneyString ignoreRunAwayEvent invincibleToHits causeHitMarker timeBeforeFireTick statusEffectMask timeOnFire stunEvent unstunEvent justStunned justUnstun justFire justOffFire timeBeforeStunTick timeBeforeStunDamage hasBounty killingChangesPlayerPrefs playerPrefsStringChanged alreadySaidShotDialogue onlyTriggerDamageAnimOnce triggeredDamageAnim triggeredDamageAnim_ damageBeforeReaction canReactToHitAgain explosionDamage fleshVelAmount shootFleshInRandomDir shrinkOnDeath 
- methods: AggroAllOtherEmployeeCopies(); AllowBrowseNPCsToStartSpawningAgain(); CanReactToHitAgain(); CancelSpawnNextNpc(); ChangeHealth(float newHP); CopyBackingFieldsToState(bool A_1); CopyStateToBackingFields(); DealStunDamage(); Delete(); DestroyAfterDelay(GameObject obj, float delay); Die(); DoHint(); Explosion(float explosionRadius); FixedUpdateNetwork(); GotARat(); HealFullHealth(); Hit(float damage, Vector3 hitFrom, bool alwaysTriggerDamageReaction = false, bool tempStun = true, string damageType = "Normal"); LerpAlphaClipping(); NetworkDestroySelf(); PlayerBlink(); Rpc_CMD_ChangeHealth(float newHP); Rpc_CMD_Die(); Rpc_CMD_Hit(float damage, Vector3 hitFrom, bool alwaysTriggerDamageReaction, bool tempStun, string damageType); Rpc_CMD_SpawnObj(string type); Rpc_ChangeHealth(float newHP); Rpc_Die(); Rpc_FireObj(bool on); Rpc_Hit(float damage, Vector3 hitFrom, bool alwaysTriggerDamageReaction, bool tempStun, string damageType); Rpc_SpawnObj(string type); Rpc_StunObj(bool on); SpawnFlesh(); SpawnObj(string type); Start(); Update(); 

## class HosePump : MonoBehaviour
- fields: pumpSpeed bulgeFrequency baseThickness bulgeThickness bulgeThicknessTarg bulgeColor waterEmitter flowSpeedMin flowSpeedMax minEmitRate maxEmitRate rope smoother time 
- methods: FixedUpdate(); LateUpdate(); OnDisable(); OnEnable(); Rope_OnBeginStep(ObiActor actor, float stepTime, float substepTime); 

## class HowToPlayNavigation : MonoBehaviour
- fields: index index firstSlideNextButton firstSlidePreviousButton lastSlideNextButton nextButtons previousButtons beginButton playerInput 
- methods: Start(); Update(); 

## class HuntManager : NetworkBehaviour
- fields: allBarricades allVents annoyedState path allEnemyHolders allEnemies huntSpawnPoints leavePoint monsterObjs monsterDifficultyPoints monsterSingleplayerDifficultyPoints jackInTheBox spiderLvl1 spiderLvl2 spiderLvl3 spiderHittable huntBar huntBarBehind maxHuntLength huntBarHolder spiderAlreadyLeft alreadyEnabledWeaponsArsenal spiderJustLeft huntInProgress enemiesSpawned spawnedJackInTheBoxAlready 
- methods: Awake(); CheckEnemiesLeft(); CopyBackingFieldsToState(bool A_1); CopyStateToBackingFields(); EnemyDied(); FixedUpdate(); GetShuffledIndices(int length); Rpc_SetValuesForClients(float maxHuntLength_, Hittable spiderHittable_); SpawnEnemies(); SpawnEnemy(int enemyType); StartHunt(); 

## class HuntPanel : NetworkBehaviour
- fields: panelCoverAnim 
- methods: Awake(); CopyBackingFieldsToState(bool A_1); CopyStateToBackingFields(); RevealPanel(); Rpc_CMD_RevealPanel(); Rpc_RevealPanel(); 

## class HygienePenalty : NetworkBehaviour
- methods: CopyBackingFieldsToState(bool A_1); CopyStateToBackingFields(); 

## class ICleanupBeforeDestroy : Il2CppObjectBase
- methods: CleanupBeforeDestroy(); 

## class IDCard : Interactable
- fields: carInspectable newCam interacting icon signature nameText dbName inspectSFX idPromptAnim checkComputerPrompt actualIDCard tutorial stopInteractEvent fakeID extraTimeBeforeFirstOccurrence playerInput isTutorialCheckoutNote _hasSpawnedSequence delayInteractable delayInteractableTime scanSfx alreadyFinishedObjective 
- methods: Awake(); CheckComputerHint(); CheckIfFakeID(); CopyBackingFieldsToState(bool A_1); CopyStateToBackingFields(); EnableInteractable(); FinishObjective(); FinishObjectives(); FinishTutorial(); HitCount(); HitsContains(PlayerRef player); HitsRemove(PlayerRef player); Interact(PlayerManager playerMan); LogCheckedID(); PayAttentionHint(); PlayScanSFX(); PlayerLeft(PlayerRef player); Render(); Rpc_CMD_PlayScanSFX(); Rpc_CMD_ScanIDToComputer(); Rpc_Interact(PlayerRef player); Rpc_PlayScanSFX(); Rpc_ScanIDToComputer(); ScanID(); ScanID(InputAction.CallbackContext context); Start(); StopInteract(); Update(); 

## class IFHierarchyIcon : Il2CppObjectBase

## class IgnoreCameraManagerDisable : MonoBehaviour
- methods: Start(); Update(); 

## static class InputConstants : Il2CppSystem.Object
- fields: k_GamepadControlScheme k_KeyboardControlScheme k_MoveActionID k_LookActionID k_JumpActionID k_InteractActionID k_SecondaryInteractActionID k_CrouchActionID k_ReloadActionID k_SprintActionID k_UseActionID k_WiggleLeftActionID k_WiggleRightActionID k_WiggleActionID k_PauseActionID k_ToggleChatHistoryActionID k_BackActionID k_NextActionID k_PreviousActionID k_NextTabActionID k_PreviousTabID k_CancelActionID k_PushToTalkActionID k_NextInventoryItemActionID k_PreviousInventoryItemActionID k_DropItemActionID k_ThrowItemActionID k_SelectActionID k_CheatsActionID k_MoveSelectionActionID k_ScrollActionID k_RotateActionID k_ScrubActionID k_ScanActionID k_DeleteSaveActionID k_SaveOptionsActionID k_NavigateActionID k_ShowLobbyPlayersActionID k_InGameActionMap k_StuckActionMap k_DialogueActionMap k_PauseMenuActionMap k_MainMenuActionMap k_ComputerActionMap k_ShelfStackingActionMap k_InterrogationActionMap k_IDCardActionMap k_LicensePlateScannerActionMap LeftStick RightStick LeftTrigger LeftBumper RightTrigger RightBumper Button_North Button_South Button_East Button_West LeftStick_Click RightStick_Click DPad DPad_Up DPad_Down DPad_Left DPad_Right Start Select GamePad 

## class InteractManager : NetworkBehaviour
- fields: mainCamera checkForInteractables interactableLayer lookAtEventLayer curInteractable prevInteractable detectDistance playerMan fpsScript promptObj promptText taskCompletion taskCompletionMax completingTaskFillAmount justStartLookingAtInteractable holdText justStopHoldInteracting playerInput longLookDistance lastLongLookHitObject triggeredLookEvents justStartHoldingInteractable justStopHoldingInteractable holdInteracting 
- methods: Awake(); ClearCurrentInteractable(); ConvertStringToKeyCode(string keyName); CopyBackingFieldsToState(bool A_1); CopyStateToBackingFields(); ForceStopInteraction(); SetCheckForInteractables(bool value); ShootLongLookRay(); ShootRay(); Start(); Update(); 

## class Interactable : NetworkBehaviour
- fields: interactable outlines interactText interactSFX interactAnim interactEvent onlyInvokeEventLocally interactCooldown useInteractCooldown holdInteractable holdInteractableTime boardedUp startInteractingEvent stopInteractingEvent curPlayerMan RequiresAllPlayersLookAt lastHits _AmountOfHits __hasCompleted cantInteractIfDowned localOnlyInteractEvent eventEvent 
- methods: CanInteract(); ChangeInteractableStatus(bool change); ChangeInteractableStatusLocalOnly(bool change); CleanupBeforeDestroy(); CopyBackingFieldsToState(bool A_1); CopyStateToBackingFields(); DestroyItself(); FirstAidKit(); Health(int x); Interact(PlayerManager playerMan); InteractableStatusEnabled(); LookAt(); OpenHatsLocker(); PlayerLeft(PlayerRef player); PurchaseWithTokens(int cost); ReInvokeInteractCooldown(); Rpc_CMD_ChangeInteractableStatus(bool change); Rpc_CMD_DestroyItself(); Rpc_CMD_Interact(PlayerRef player); Rpc_CMD_ReInvokeInteractCooldown(); Rpc_ChangeInteractableStatus(bool change); Rpc_Interact(PlayerRef player); Rpc_ReInvokeInteractCooldown(); Start(); StopLookAt(); 

## class InventoryManager : NetworkBehaviour
- fields: index type pos rot playerMan aId bId curInventorySlot maxInventorySlots inventoryIds inventoryAmounts inventoryAmountTexts inventoryAmountTextFades holdingObjs itemCanvases holdingAnims dropAnchors maxStack objSprites inventorySprites inventorySprites_ inventorySlots emptySprite pistolAnim shotgunAnim holdingIndex throwAnchor playerCol justThrownCol throwObstacle characterController collidersToCheck playerCam camShake recoil canShoot canControlItem playerMan flashlightLight shootable cleanable hitParticle mopAnim hasTrash itemStorages itemStorages2 returnObj trash remoteTraps trashBagAnim killMarker hitMarker mopReturned pistolDryShot taskCompletion taskCompletionMax completingTaskFillAmount completingBoardFillAmount completingExplosiveFillAmount completingHeal bearTrapTemplate bearTrapTemplateRed landmineTemplate landmineTemplateRed stunMineTemplate stunMineTemplateRed explosiveTemplate explosiveTemplateRed posterTemplate pottedPlantTemplate pottedPlantTemplateRed waterCoolerTemplate waterCoolerTemplateRed basketRackTemplate basketRackTemplateRed atmTemplate atmTemplateRed mailboxTemplate mailboxTemplateRed trashCanTemplate trashCanTemplateRed bannerTemplate bannerTemplateRed floorMatTemplate floorMatTemplateRed sunglassesRackTemplate sunglassesRackTemplateRed booksTemplate booksTemplateRed bobbleHeadTemplate bobbleHeadTemplateRed burgerTemplate burgerTemplateRed plant1Template plant1TemplateRed plant2Template plant2TemplateRed plant3Template plant3TemplateRed plant4Template plant4TemplateRed robotTemplate robotTemplateRed boomboxTemplate boomboxTemplateRed gumballTemplate gumballTemplateRed clockTemplate clockTemplateRed ivyTemplate ivyTemplateRed stringLightsTemplate stringLightsTemplateRed painting1Template painting1TemplateRed painting2Template painting2TemplateRed painting3Template painting3TemplateRed deerTemplate deerTemplateRed trapObstacles placedLandmine placedStunMine placedbearTrap placedExplosive placedPoster placedPottedPlant placedWaterCooler placedBasketRack placedATM placedMailbox placedTrashcan placedBanner placedFloorMat placedSunglassesRack placedBooks placedBobbleHead placedBurger placedPlant1 placedPlant2 placedPlant3 placedPlant4 placedRobot placedBoombox placedGumball placedClock placedIvy placedStringLights placedPainting1 placedPainting2 placedPainting3 placedDeer letGoOfInteract letGoOfClick explosiveHeldAnim landmineHeldAnim stunMineHeldAnim bearTrapHeldAnim plankHeldAnim interactMan curBarricade promptObj blocksPlacementLayerMask smallItemPlacementLayerMask posterPlacementLayerMask alreadyPlacing alertedTheCreatureWarning justStartPlacingTrap explosiveRemote thirdPersonMan playerShootPoint flamethrowerShootPoint charController downed hasThrownIntoPulverizerBefore tasking emotiscopeSearching emotiscopeScanning emotiscopeFound emotiscopeLayer emotiscopeEmotionText scanningDataSfx scanBar curScan flashlightToggle gunJammed gunVines gasPumpTrigger duckAnim gasPumpOrigin gasPumpCurDistance gasPumpGasParticles gasPumpSmokeParticles gasPumpShootSfx gasPumpEmptySfx thirdPersonGasPump petrolTankLayer inventoryPaused inventoryUIHolder healthUIHolder shotgunAmmoText flamethrowerAmmoText hoseAmmoText flamethrowerAmmo flamethrowerThrown smgStashedAmmoText smgMagAmmoText smgAnim reloadSmgBar hasGun flamethrowerFireLoop playerInput _templatesAssigned pendingPickup pendingPickupIndex pendingPickupAmount pendingPickupAmount2 pendingHoldingIndex pendingSendHoldingIndex justStartedGasPump justStoppedGasPump rotationIndex currentInventoryIndex barricadeHoldStartTimer BarricadeHoldStartDelay barricadeHoldStarted chainsawAudio chainsawScrollTexture curTimeForcedGhostCam timeForcedGhostCam objEnabledFromGhostCamera justHold justStopHold ghostAnim filledBucketDamageCooldown speedBoostParticles hoseSfx hoseSpray timeBeforeHoseAgain canAttack justStopLookingAtDi lastDi pistolBulletIcons reloading reloadBar reloadRoutine shotgunBulletIcons reloadShotgunBar reloadRevolverBar revolverBulletIcons revolverAnim flamethrowerFlame timeUntilLoseFlamethrowerAmmo curEnemyShotIndex meleeHitParticles justSwitchedInventorySlots _playerId objectIdsThatDontRemainBetweenShifts isTransitioning cache_playerId 
- methods: AddTrash(); AlertSameEnemy(); AskMood(); AssignTemplates(bool forceAssignment = false); AutoLetGoOfClick(); Awake(); BeginSceneTransition(); CanAttack(); CanShoot(); CancelAllInvokes(); CancelBarricadeTask(); CancelReload(); ChangeHasGun(bool hasGun_); ChangeInventorySlot(int slot); ChangePlayerItemStorage(int invSlot, int value, int value2); CheckConstrictedInteractables(); CheckIfRemotesStillOn(); CleanSpill(); CleanSpillHose(); ConvertStringToKeyCode(string keyName); CopyBackingFieldsToState(bool A_1); CopyStateToBackingFields(); DelayedEquipFromInventory(int index); DestroyObject(); DisableJustSwitchedInventorySlots(); DrinkCobraCola(); DropEverything(bool dropTrash = true); DropObject(); EnableGhostCamObj(); Explode(); FinishReturnObject(); FullyResetBarricadeHold(); GetXZDistance(Transform a, Transform b); GunJam(); GunUnjam(); IgnoreCollision(NetworkObject a, NetworkObject b); IsValidItemIndex(int index); LoadInventoryFromLastSave(); MeleeAttack(float damage, int hitParticle, float attackRange); NormalizeFallback(string value); PauseInventory(); PauseUseItem(); PickupNewObj(int index, int itemStorage, int itemStorage2 = 0); PlaceItem(PlayerManager playerMan, string type, Vector3 pos, Quaternion rot); PlayAddTrashAnim(); QueuePickup(int index, int amount, int amount2); Reload(); ReloadRevolver(); ReloadSMG(); ReloadShotgun(); Rpc_CMD_ChangeHasGun(bool hasGun_); Rpc_CMD_ChangeItemStorage(int invSlot, int value, int value2); Rpc_CMD_GotARat(); Rpc_CMD_PlaceItem(PlayerManager playerMan, string type, Vector3 pos, Quaternion rot); Rpc_CMD_RequestIgnoreCollision(NetworkId aId, NetworkId bId); Rpc_CMD_SendIDToServer(string newID); Rpc_CMD_UpdateCurInventorySlot(int slot); Rpc_CMD_UpdateHoldingIndex(int index); Rpc_CMD_UpdateInventoryForHost(Il2CppStructArray<int> inventoryIds_, Il2CppStructArray<int> inventoryAmounts_, Il2CppStructArray<int> trash_); Rpc_ChangeHasGun(bool hasGun_); Rpc_ChangeItemStorage(int invSlot, int value, int value2); Rpc_DoIgnoreCollision(NetworkId aId, NetworkId bId); Rpc_GotARat(); Rpc_Pulverized(); Rpc_RegisterRemoteTrap(NetworkObject trapObj, int slot); Rpc_SetMaxInventorySlots(int slots); Rpc_StopIgnoreCollision(NetworkId aId, NetworkId bId); Rpc_UpdateCurInventorySlot(int slot); Rpc_UpdateHoldingIndex(int index); Rpc_UpdateInventoryForAll(Il2CppStructArray<int> inventoryIds_, Il2CppStructArray<int> inventoryAmounts_, Il2CppStructArray<int> trash_); SafeSetActive(Transform t, bool active); SetEmotionText(DialogueInteractable dialogueScript); SetRemoteTrap(RemoteTrap trap); Shoot(float damage); ShootFlamethrower(); ShootSMG(float damage); ShootShotgun(float damage); Spawned(); SpeedBoost(); SpeedBoostFinish(); StopIgnore(NetworkId aId, NetworkId bId); ThrowObject(); ToggleBearTrapRadii(bool on); ToggleLandmineRadii(bool on); ToggleStunMineRadii(bool on); TokenHint(); TryGetCleanable(Collider c, out Spill spill, out Moppable mop); TryGetCollider(NetworkObject obj, out Collider col); TurnOffTaskingObjs(); UnpauseInventory(); UnpauseUseItem(); Update(); UpdateCurInventorySlot(int slot); UpdateHoldingIndex(int index); UpdateInventorySlotsUI(); 

## class JSONAccess : MonoBehaviour
- fields: name values key value entries remaining onComplete category filePath fileName category lang onDone fileName category lang k_textNotFoundString k_tnfString languageFonts _cacheByCategory _cachedLanguageByCategory _cacheLoadedCategories LMB RMB 
- methods: Awake(); EnsureCacheLoaded(string category, string lang); GetCarDatabaseInnerName(string outerName); GetCarDatabaseText(string id, string key); GetDialogueText(string id, string key); GetFileName(string category, string lang); GetIDDatabaseText(string id, string key); GetMiscText(string id, string key); GetSafeCharacterDescription(char c); GetShowingName(string name); GetStatus(string name); GetTextFromCategory(string category, string id, string key); LoadCategoryFromEncryptedText(string category, string encryptedB64); LoadCategoryFromText(string category, string textFromFile); NormalizeNameKey(string input); PreloadAllForCurrentLanguage(Il2CppSystem.Action onComplete); PreloadCarDatabaseAsync(string lang); PreloadCategoryAsync(string category, string lang); PreloadCategoryAsyncCallback(string category, string lang, Il2CppSystem.Action onDone); PreloadDialogueAsync(string lang); PreloadIDDatabaseAsync(string lang); PreloadMiscAsync(string lang); ReloadLmbString(); ShouldSkipDecryption(); Start(); TryGetCarDatabaseEntryDict(string plate, out Dictionary<string, string> dict); TryGetCarDatabaseNames(out List<string> names); TryGetEntryDictionaryFromDialogue(string id, out Dictionary<string, string> dict); TryGetIDDatabaseEntryDict(string id, out Dictionary<string, string> dict); TryGetIDDatabaseNames(out List<string> realNames, out List<string> showingNames); TryParseDialogueData(string json, out DialogueData parsed, out string failureReason); 

## class JackInTheBox : Interactable
- fields: box jackInTheBoxAudio jackInTheBoxAnim jackInTheBoxPositions timeRunning paused opened marionette 
- methods: Awake(); BoxReappear(); CopyBackingFieldsToState(bool A_1); CopyStateToBackingFields(); FixedUpdate(); ResetBox(); Rpc_CMD_FullyUnwindJackInTheBox(); Rpc_CMD_PauseJackInTheBox(); Rpc_CMD_UnpauseJackInTheBox(); Rpc_FullyUnwindJackInTheBox(); Rpc_PauseJackInTheBox(); Rpc_TeleportBox(Vector3 newPos, float yEuler, float timeBeforeReappear); Rpc_UnpauseJackInTheBox(); 

## class JumpscarePlayer : MonoBehaviour
- fields: anim goal moveSpeed turnSpeed snapDistance snappedToGoal snapBackToPosition 
- methods: Finish(); FixedUpdate(); OnEnable(); SnapToGoal(); 

## class KeybindsManager : MonoBehaviour
- fields: buttonLabels buttonBGs keyIndex picking sensSlider sensValue sfxValue musicValue playerVolValue inputDropdown micPickerDropdown resolutionDropdown lostConnectionWarning k_uitext4 k_soundOffDBValue playerInput invertX invertY epilepsyMode shootLights shootSprites arachnophobiaMode fovSlider fovValue brightnessSlider brightnessValue globalVolume colorAdj camShakeToggle camBobbingToggle fullscreenDropdown frameRateDropdown vSyncToggle sfxSlider musicSlider playerVolSlider audioMixer SFXVolumeParameter MusicVolumeParameter PlayerVolumeParameter 
- methods: ApplyEV(float ev); ApplyFrameRate(int index); ApplyFullscreenMode(int modeIndex); ApplyVSync(bool isOn); ChangeArachnophobiaMode(); ChangeEpilepsyMode(); ChangeInvertX(); ChangeInvertY(); ChangeKey(int num); InitializeKeybinds(); LoadKeybindsMenu(); LoadSettings(); OpenFeedback(); RestoreAllControlsSettings(); RestoreAudioSettings(); RestoreControlsSettings(); RestoreVideoSettings(); SaveSettings(string key, float value); SaveSettings(string key, int value); SetDefaultKeybinds(); SetFrameRate(int index); SetFullscreenMode(int modeIndex); SetMusicVolume(float volume); SetPlayerVoiceVolume(float volume); SetSFXVolume(float volume); SetVSync(bool isOn); Start(); ToggleCamBobbing(); ToggleCamShake(); Update(); UpdateAllLanguageChanges(); UpdateBrightness(); UpdateFOV(); UpdateResolutionDropDown(); UpdateSensitivity(); 

## class KickPlayerMenu : MonoBehaviour
- fields: kickPlayerPrefab kickPlayerListParent kickPlayerList curPlayerRefToKick confirmKickPlayerWindow confirmKickPlayerText 
- methods: AddAllPlayersToList(); AskToKickPlayer(PlayerRef playerRef, string playerString); ClearPlayerList(); ConfirmKickPlayer(); OnEnable(); RefreshPlayerList(); 

## class KickPlayerPrefab : MonoBehaviour
- fields: nameText kickButton targetPlayerRef menu 
- methods: Kick(); SetPlayer(string playerName, PlayerRef playerRef, KickPlayerMenu kickMenu); 

## class KwaleeEULA : MonoBehaviour
- fields: declineButton acceptButton loadingScreen k_photosensitivityWarningScene playerInput _buttonFocusRoutine k_timeTilAcceptAvailable 
- methods: AllowAcceptance(); Awake(); KeepButtonFocus(); LoadNextScene(); OnEULAAccepted(); OnEULARejected(); OnSelectPerformed(InputAction.CallbackContext context); 

## class Landmine : NetworkBehaviour
- fields: monsterCheckScript cantPlaceRadius curCantPlaceRadius radiusPos interactable landmineBeep exploded explosion landmineMesh 
- methods: ActuallyDestroy(); CopyBackingFieldsToState(bool A_1); CopyStateToBackingFields(); DestroyAfterTime(float time); DisableRadius(); EnableRadius(); Rpc_CMD_Trap(); Rpc_Trap(); SpawnExplosion(); Start(); Trap(); 

## class LanguageManager : MonoBehaviour
- fields: savedLang dropdown micPicker keybindsManager 
- methods: OnLanguageChanged(int index); OnLanguageLoaded(); Start(); 

## class LanguageSelect : MonoBehaviour
- fields: k_autosaveSceneName playerInput previousFrameWasController 
- methods: EnableCursor(); NextScene(); OnEnable(); Start(); Update(); 

## class LanguageText : MonoBehaviour
- fields: useTextComponentAsKey id key resetEveryEnable fontOnly xboxOverrideKey keyForUseTextComponentAsKey tmp playerInput glyphTextToSpriteID k_spriteName 
- methods: Awake(); OnControlsChanged(PlayerInput input); OnDestroy(); OnEnable(); SetText(); Start(); 

## class LastSelectedObject : MonoBehaviour
- fields: _instance _playerInput _lastSelectedObj _switchToKeyboardThisFrame _switchToControllerThisFrame 
- methods: GetEventSystemRaycastResults(); IsPointerOverUIElement(); IsPointerOverUIElement(List<RaycastResult> eventSystemRaysastResults); Start(); Update(); 

## class LemonadeStand : NetworkBehaviour
- fields: pubertKilled pubertNotKilled 
- methods: CopyBackingFieldsToState(bool A_1); CopyStateToBackingFields(); Rpc_EnablePubert(bool killed); 

## class LobbyMenu : MonoBehaviour
- fields: started cam camCenter busAudio lobbyMenu versionText playerInput playButton 
- methods: Awake(); Start(); Update(); 

## static class LobbyPasswordUtility : Il2CppSystem.Object
- methods: HashPasswordForLobby(string lobbyId, string password); 

## class LobbyUIManager : NetworkBehaviour
- fields: localUID handler localUID xuid resolvedXuids h player Instance LobbyHolderObject waitingOnHostIndicator unableToJoinLobbyHolder inviteFriendsButton playerListParent playGameButton leaveGameButton shirtColourButton shirtColourManager lobbyTitleText settingsBTN mainMenuTrack playerNameTexts playerLobbyHandlers camTranslation canvas cutsceneObj fadeOut inCutscene LobbyPlayers HostInfo leaveGameButtonObject isLeaving playerReadyButton gamerTagsScreen playerInput showPlayersInLobbyCallout lobbyLoadingObj lobbyMainLoadingObj backButtonPressed _moveToGame _retryUpdatePending _retryCount k_maxRetryUpdateCount k_sortNamesDelay leaveButton leavingUI playButton 
- methods: Awake(); CancelMoveToGame(); CheckAllPlayersReady(); CopyBackingFieldsToState(bool A_1); CopyStateToBackingFields(); DelayedStart(); FadeOut(); FixedUpdate(); HostLobby(); InviteFriends(); LeaveGame(); LoadGame(); Log(string msg); OnPlayButtonClicked(); OnShowLobbyPlayersPressed(InputAction.CallbackContext context); RegisterPlayer(PlayerLobbyHandler player); RetryUpdate(); Rpc_SetPlayButtonInteractable(bool allPlayersReady); Rpc_StartCutscene(); SetLoadingText(); SortNamesAfterDelay(); Start(); StartCutscene(); TrySetGamertagFromPlatform(PlayerLobbyHandler player); Update(); UpdatePlayerLobbyUI(); 

## class LogAllSceneIds : MonoBehaviour
- methods: Start(); 

## class LookAtPlayer : MonoBehaviour
- fields: cam lockXRot 
- methods: FixedUpdate(); 

## class MainMenu : MonoBehaviour
- fields: started cam camCenter busAudio playMenu mainMenu selectSaveFileMenu playButton versionText playerInput networkErrorObj networkErrorObjTitleText networkErrorObjDescText _errorMessageShowing _disconnectPopupShown joinScreenOverlay resolutionDropdown createLobbySettingsMenu 
- methods: Awake(); CreateLobby(); Discord(); DismissErrorMessage(); GetErrorKey(NetworkErrors _errorType); HideJoining(); LoadGame(); LoadMenu(); LoadPetCutscene(); MoveToLobby(); OnDisable(); OnEnable(); QuitGame(); ReUnlockCursor(); SetAsSoloMode(bool isSolo); ShowDisconnectPopup(NetworkErrors errorType); ShowDisconnectedMessage(bool show, NetworkErrors _errorType); ShowJoining(); Start(); StartGame(); Update(); Wishlist(); 

## class MainMenuMusic : MonoBehaviour
- methods: Awake(); 

## class Marionette : Enemy
- fields: spiderSpawn anim huntMan playerMans enemyHolder nearestPlayer matFader maxHealth justFadeIn justFadeOut wallDetect wallLayerMask isHitting timeUntilAttack _NetworkedPosition _NetworkedRotation _IsPositionInitialised _hasSnappedToInitialPosition 
- methods: CheckEnemiesLeft(float timeUntilCheck); CompleteHunt(); CopyBackingFieldsToState(bool A_1); CopyStateToBackingFields(); DetectPlayers(); FixedUpdate(); FixedUpdateNetwork(); GoToTarget(); Render(); Rpc_ChangeHittableHealth(float health); Spawned(); Start(); TakeDamage(); UpdatePlayerLists(); 

## class MaterialFader : MonoBehaviour
- fields: duration duration autoFadeInDuration _mat _baseColor _active BaseColor Surface SrcBlend DstBlend ZWrite 
- methods: Awake(); FadeIn(float duration); FadeOut(float duration); GetBaseColorAlpha(); PlayFadeIn(float duration); PlayFadeOut(float duration); SetBaseColorAlpha(float a); SetSurfaceOpaque(); SetSurfaceTransparent(); Start(); Start_(); 

## class MaxWidthLayoutElement : MonoBehaviour
- fields: maxWidth _le 
- methods: Apply(); Awake(); CalculateLayoutInputHorizontal(); CalculateLayoutInputVertical(); OnEnable(); OnValidate(); 

## class MicAudioCanvas : MonoBehaviour
- fields: openMic activationMic pttMic voiceConnection _recorder speakingAmpThreshold sensitivity enableCustomPttHold customPttKey playerInput PrefKey 
- methods: Awake(); ConvertStringToKeyCode(string keyName); OnDestroy(); OnVoiceConnectionReady(VoiceConnection vc); SetCurrentMode(InputMode mode); Start(); Update(); 

## class MicLoudnessMonitor : MonoBehaviour
- fields: voiceConnection bar sensitivity decayRate _recorder _lastSentScent k_ScentSendThreshold 
- methods: Awake(); Start(); Update(); 

## class MicPickerDropdown : MonoBehaviour
- fields: dropdown voiceConnection includeSystemDefault _devices PrefKey 
- methods: ApplySavedSelectionIfAny(); ApplySelection(int index, bool save); Awake(); BuildList(); OnAudioConfigChanged(bool deviceWasChanged); OnDestroy(); OnDropdownChanged(int index); Start(); UpdateAllLanguageChanges(); 

## class MiscHelpers : MonoBehaviour
- methods: Flatten(List<List<int>> source, out Il2CppStructArray<int> flat, out Il2CppStructArray<int> lengths); Unflatten(Il2CppStructArray<int> flat, Il2CppStructArray<int> lengths); 

## class MonsterCheckEvent : MonoBehaviour
- fields: monsterCheckPosition distanceToAttract 
- methods: CauseDistraction(); GetXZDistance(Transform a, Transform b); 

## class MonsterStatue : MonoBehaviour
- fields: creepyPainting 
- methods: Awake(); 

## class Moppable : NetworkBehaviour
- fields: spawnObj col mopReason mopRevenue roach 
- methods: CopyBackingFieldsToState(bool A_1); CopyStateToBackingFields(); RequestClean(); Rpc_ActuallyClean(); Rpc_CMD_Clean(); ServerActuallyClean(); 

## class MoveCamera : MonoBehaviour
- fields: turnSpeed panSpeed zoomSpeed mouseOrigin isPanning isRotating isZooming 
- methods: Update(); 

## class MovePath : MonoBehaviour
- fields: startPos finishPos w targetPoint targetPointsTotal animName walkSpeed runSpeed loop forward walkPath randXFinish randZFinish _overrideDefaultAnimationMultiplier _customWalkAnimationMultiplier _customRunAnimationMultiplier 
- methods: InitializeAnimation(bool overrideAnimation, float walk, float run); MyStart(int _w, int _i, string anim, bool _loop, bool _forward, float _walkSpeed, float _runSpeed); SetLookPosition(); Start(); Update(); 

## class MultiplayerButton : NetworkBehaviour
- fields: otherButton selectionEvent amountOfPlayersText selectionBar selectionBarBG clientPlayerPicked totalPlayers playersSelected curTimeSelecting maxTimeSelecting onlySpawnIfXFunds xFunds thisButton _hasSelected 
- methods: CopyBackingFieldsToState(bool A_1); CopyStateToBackingFields(); FixedUpdate(); PlayerInteractedWithButton(); Rpc_CMD_PlayerInteractedWithButton(bool selected); Rpc_PlayerInteractedWithButton(bool selected); Rpc_SetTotalPlayers(int players); SelectButton(); Start(); 

## class NPCFolder : MonoBehaviour
- fields: nameText descText idPhoto killedIcon 

## sealed class NetMultiListData
- fields: _data 

## sealed class NetPlayerData : Il2CppSystem.ValueType
- fields: _UID _Gamertag 

## enum NetworkErrors

## struct NetworkInputData
- fields: move look yawDelta jump sprint crouch 
- methods: BoxIl2CppObject(); 

## class NewPath : MonoBehaviour
- fields: points pointLenght mousePos pathName errors exit par PathType 
- methods: PointSet(int index, Vector3 pos); PointsGet(); 

## class NodeSwitch : NetworkBehaviour
- fields: enableNodes disableNodes nodeTriggerEvent runsTriggerAsRpc 
- methods: CopyBackingFieldsToState(bool A_1); CopyStateToBackingFields(); EnterCutscene(); Rpc_CMD_TriggerSwitch(); Rpc_TriggerSwitch(); TriggerSwitch(); TriggerSwitchLocal(); 

## class NonCustomerDialogueInteractable : MonoBehaviour
- methods: Start(); Update(); 

## class Npc : ScriptableObject
- fields: id prefab isDoppelganger entertainment difficulty mustBeAliveToSpawn mustHaveSpawnedBefore onlySpawnAfterThisDay onlySpawnBeforeThisDay alwaysOnlySpawnOnThisDay alwaysOnlySpawnOnThisIndex extraTimeBeforeSpawn 

## class NuisanceCustomer : NetworkBehaviour
- fields: matFaders target speed runSpeed seeker pathfinder hittable anim idleAnim walkAnim runAnim fastRunAnim damageAnim deathAnim curAnim takenDamage dropBloodParticles hitParticles emotionIcon nuisanceType curNuisanceAnimation curNuisanceObject beingANuisance nuisanceSprite decreaseNuisanceLoopingParticle increaseNuisanceParticle objectAtlases exitCoroutine startedFadingAway _NetworkedPosition _NetworkedRotation _IsPositionInitialised _hasSnappedToInitialPosition 
- methods: ChangeSpeed(float newSpeed); CopyBackingFieldsToState(bool A_1); CopyStateToBackingFields(); DelayedStart(); Die(); EnableBeingANuisance(); FixedUpdateNetwork(); GoToExit(); GoToTarget(); IdleAnim(); InstantiateIncreaseParticle(); InvokeTookDamage(); OnTriggerEnter(Collider other); RegularAnim(); Render(); RevertToCurSpeed(); Rpc_CMD_ChangeSpeed(float newSpeed); Rpc_CMD_RevertToCurSpeed(); Rpc_CMD_Trigger(string triggerName); Rpc_ChangeSpeed(float newSpeed); Rpc_EndNuisance(); Rpc_RevertToCurSpeed(); Rpc_StartNuisance(int nuisanceType_); Rpc_Trigger(string triggerName); RunOutOfStore(); SelectNuisanceType(); Spawned(); StartRunning(); TookDamage(bool stunWhenHit = true); TriggerAnim(string triggerName); WalkOutOfStore(); 

## class NuisanceCustomerManager : MonoBehaviour
- fields: nuisancesAlreadyBeingDone nuisanceAnimations nuisanceObjects sceneAssetsToDisableOnNuisance allOutsideNuisancePoints allInsideNuisancePoints dancingPoints graffitiPoints swirlyPoint ringBellPoint overheadSquatPoint 
- methods: Awake(); 

## class ObiActorTeleport : MonoBehaviour
- fields: actor target 
- methods: Teleport(); 

## class ObiParticleCounter : MonoBehaviour
- fields: solver counter targetCollider frame particles 
- methods: Awake(); OnDisable(); OnEnable(); Solver_OnCollision(Il2CppSystem.Object sender, ObiNativeContactList e); 

## class ObjectAtlas : MonoBehaviour
- fields: objects 

## class ObjectDragger : MonoBehaviour
- fields: screenPoint offset 
- methods: OnMouseDown(); OnMouseDrag(); 

## class ObjectEnabledFromGhostCamera : MonoBehaviour
- fields: timeForcedGhostCam 
- methods: Start(); 

## class ObjectLimit : MonoBehaviour
- fields: minX maxX minY maxY minZ maxZ 
- methods: Update(); 

## class Outline : MonoBehaviour
- fields: data registeredMeshes outlineMode outlineColor outlineWidth precomputeOutline bakeKeys bakeValues renderers outlineMaskMaterial outlineFillMaterial needsUpdate 
- methods: Awake(); Bake(); CombineSubmeshes(Mesh mesh, Il2CppReferenceArray<Material> materials); FixedUpdate(); LoadSmoothNormals(); OnDestroy(); OnDisable(); OnEnable(); OnValidate(); SmoothNormals(Mesh mesh); Update(); UpdateMaterialProperties(); 

## class Panel : MonoBehaviour
- fields: PanelName 

## class ParticleCollisionSpawner : NetworkBehaviour
- fields: objectToSpawn particleSystem spawnEveryXCollisions curSpawnEveryXCollisions 
- methods: CopyBackingFieldsToState(bool A_1); CopyStateToBackingFields(); OnParticleCollision(GameObject other); Start(); 

## enum PathType

## class PauseMenu : MonoBehaviour
- fields: oldSelectedObj playerInput subMenus backPressed inSubMenu controllerDisconnectPopup 
- methods: Awake(); BackButtonPressed(); BackPressed_Internal(); OnCancelPressed(InputAction.CallbackContext context); OnCancelQuit(); OnDisable(); OnEnable(); OnMenuOptionSelected(GameObject startSelectedObject); OnQuitPressed(GameObject startSelectedObject); 

## static class PendingLobbySettings : Il2CppSystem.Object
- fields: lobbyType maxPlayers lobbyPassword lobbyName gamemode night 

## sealed class PendingSpawnCallback : Il2CppSystem.ValueType
- fields: _obj _callback _position _rotation 

## class PeopleController : MonoBehaviour
- fields: timer animNames damping target 
- methods: SetAnimClip(string animName); SetTarget(Vector3 _target); Start(); Tick(); Update(); 

## class PeopleWalkPath : WalkPath
- fields: _moveType direction walkSpeed runSpeed isWalk _overrideDefaultAnimationMultiplier _customWalkAnimationMultiplier _customRunAnimationMultiplier 
- methods: DrawCurved(bool withDraw); SpawnOnePeople(int w, bool forward, float walkSpeed, float runSpeed); SpawnPeople(); 

## class PerPlayerVolumeRow : MonoBehaviour
- fields: playerNameText volumeSlider volumePercentageText username onVolumeChanged isConfiguring 
- methods: Awake(); Configure(string playerUsername, float startingVolume, Il2CppSystem.Action<string, float> volumeChangedCallback); HandleSliderChanged(float volume); OnDestroy(); SetVolumeWithoutNotification(float volume); UpdatePercentageText(float volume); 

## class PerPlayerVolumeUI : MonoBehaviour
- fields: player row voiceConnection listRoot rowPrefab _rows _runner 
- methods: Awake(); BuildInitial(); FindSpeakerForPlayer(PlayerRef player); OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason); OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, Il2CppStructArray<byte> token); OnConnectedToServer(NetworkRunner runner); OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, Il2CppSystem.Object> data); OnDisable(); OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason); OnEnable(); OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken); OnInput(NetworkRunner runner, NetworkInput input); OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input); OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player); OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player); OnPlayerJoined(NetworkRunner runner, PlayerRef player); OnPlayerLeft(NetworkRunner runner, PlayerRef player); OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress); OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, Il2CppSystem.ArraySegment<byte> data); OnSceneLoadDone(NetworkRunner runner); OnSceneLoadStart(NetworkRunner runner); OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList); OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason); OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message); TryAddRow(PlayerRef player); WaitForSpeaker(PlayerRef player, PerPlayerVolumeRow row); 

## class Pet : NetworkBehaviour
- fields: anim allFlesh targetFlesh wanderTarget seeker pathfinder speed currentlyEatingFlesh trackedFleshTarget trackedFleshStartTime MaxTimePerFleshTarget petNameText petName wanderingCooldown _NetworkedPosition _NetworkedRotation _IsPositionInitialised _hasSnappedToInitialPosition 
- methods: BeginEatingCurrentFlesh(); CanWanderAgain(); CopyBackingFieldsToState(bool A_1); CopyStateToBackingFields(); EatCurrentFlesh(); FindDifferentFleshPiece(); FinishEatingFlesh(); FixedUpdate(); FixedUpdateNetwork(); NewFleshPiece(Trash piece); Render(); Rpc_CMD_UpdatePetAnim(string anim_, bool on); Rpc_EatAnim(); Rpc_SetPetName(string n); Rpc_UpdatePetAnim(string anim_, bool on); SetPetNameDelayed(); Spawned(); TrackCurrentFleshTarget(); WanderAroundAimlessly(); 

## class PetImage : MonoBehaviour
- fields: img happy sad sick sick2 sleepy happySprites sadSprites sickSprites sick2Sprites sleepySprites 
- methods: Start(); 

## class PetSelectorInputManager : MonoBehaviour
- fields: playerInput selectPetButtons wasUsingGamepad 
- methods: Start(); Update(); 

## class PetrolTank : NetworkBehaviour
- fields: filledBar moneyText filledMoneyText petrolFillSfx curMoneySpent maxMoneySpent pumping petrolFullSfx petrolTankDoor petrolTankDoorAnim leaveDialogueOption uiCanvas beforeUI afterUI petrolCollider petrolFull 
- methods: CopyBackingFieldsToState(bool A_1); CopyStateToBackingFields(); FixedUpdate(); PetrolPumped(); Rpc_CMD_PetrolPumped(); Rpc_PetrolPumped(); SetPetrolColliderEnabled(bool enabled); Start(); StopPumping(); 

## class PhotosensitivityScreen : MonoBehaviour
- fields: k_cutsceneName k_delayMs 
- methods: Start(); 

## class PickupObject : Interactable
- fields: collectAnimator objectIndex col destroyAfterPickup turnOffPickup hitSFXArray hitSFXLayers itemStorage itemStorage2 amountText amountText2 unableToPickupTwice timeBeforeDestroy _pendingItems _pendingItems2 ignoreAutoDisableInteractable _spawned interactabilityBeforePaused saveSnapShotObj forceThreshold maxForce minVolume maxVolume maxPitch minPitch hitAudios index 
- methods: ChangeAmountOfItems(int x, int y); CopyBackingFieldsToState(bool A_1); CopyStateToBackingFields(); Delete(); DestroyAfterTime(); Interact(PlayerManager playerMan); IsInLayerMask(GameObject obj, LayerMask mask); LoadAmountText(); LoadPendingItems(); OnCollisionEnter(UnityEngine.Collision collision); Rpc_CMD_ChangeAmountOfItems(int x, int y); Rpc_ChangeAmountOfItems(int x, int y); Rpc_Interact(PlayerRef player); Rpc_PlayAudio(float volume, int audioIndex); Spawned(); Start(); 

## class PlatformManager : MonoBehaviour
- fields: onComplete _inString slot data slot ENABLE_DEBUG_PLATFORM _lobbyID s_MAXPLAYERS _unlockedAchievements OnNetworkDisconnected OnJoiningSession OnJoiningSessionFailed _currentNetworkReachability _networkMonitor k_networkMonitorPeriod consoleInputBlackList isUsingSoloMode playerInput _pendingMenuError _pendingMenuErrorIsFromSuspendResume _connectivityCheckExcludedScenes m_bInitialized OnServerBrowserLobbiesUpdated OnServerBrowserLobbyNoLongerExists 
- methods: Awake(); ChangeFromLobbyToGame(); CheckStringForProfanityAsync(string _inString, Il2CppSystem.Action<string> onComplete); CheckVoicePermissionsForPlayer(string userId); ClearAchievement(string achievementID); CreatePlatformManager(); DoesServerBrowserPasswordMatch(string lobbyId, string passwordAttempt); FireJoiningSession(); FireJoiningSessionFailed(); FireNetworkDisconnected(NetworkErrors errorType); FireServerBrowserLobbiesUpdated(List<BrowserLobbyData> lobbies); FireServerBrowserLobbyNoLongerExists(); ForceLobbyRefresh(); FusionReadyForLobbyJoins(); GetCurrentLobbyName(); GetHostInfo(); GetLobbyMembers(); GetPlayerGamertag(); GetPlayerProfilePic(ProfilePicSize size = ProfilePicSize.Medium); GetPlayerUID(); GetPlayerUIDForFusionPlayerId(int playerId); GetPreferredFusionHostRegion(); HandleEndSessionReturn(NetworkErrors _errorType); HasInternetConnection(); HostLobby(); Init(); IsANetworkPlayScene(string activeScene); JoinServerBrowserLobby(string lobbyId, string passwordAttempt); LoadData(int slot); LoadPlayerPrefs(); MonitorNetworkConnection(); OnDestroy(); RefreshAllVoicePermissions(); RefreshServerBrowserLobbies(); RequestCursorLock(); SaveData(string data, int slot); SavePlayerPrefs(); SetLobbyRegion(string region); SetSoloMode(bool setSolo); ShowFriendsUI(); ShowGamerCard(ulong playerId); StartNetworkMonitoring(); UnlockAchievement(Achievements.AchievementID achievementID); Update(); add_OnJoiningSession(Il2CppSystem.Action value); add_OnJoiningSessionFailed(Il2CppSystem.Action value); add_OnNetworkDisconnected(Il2CppSystem.Action<NetworkErrors> value); add_OnServerBrowserLobbiesUpdated(Il2CppSystem.Action<List<BrowserLobbyData>> value); add_OnServerBrowserLobbyNoLongerExists(Il2CppSystem.Action value); isSoloMode(); remove_OnJoiningSession(Il2CppSystem.Action value); remove_OnJoiningSessionFailed(Il2CppSystem.Action value); remove_OnNetworkDisconnected(Il2CppSystem.Action<NetworkErrors> value); remove_OnServerBrowserLobbiesUpdated(Il2CppSystem.Action<List<BrowserLobbyData>> value); remove_OnServerBrowserLobbyNoLongerExists(Il2CppSystem.Action value); 

## class PlatformManager_Steam : PlatformManager
- fields: delay delay _errorType _errorType frameCount ENABLE_DEBUG_STEAM _lobbyCreated _gameLobbyJoinRequested _lobbyEntered _lobbyChatUpdate _lobbyDataUpdated _currentLobby _pendingJoinLobby _hostAddressKey _isLeavingLobby _isEndingSession _joinResolved _joinTimeoutCoroutine _turkeyFallbackHostCountries _joiningFromServerBrowser _serverBrowserJoinWatchdogCoroutine _lobbyMatchListResult _hostLobbyPassword _lobbyNameKey _browserVisibleKey _passwordProtectedKey _passwordHashKey _gamemodeKey _nightKey _maxPlayersKey _playerCountKey _versionKey _gameStartedKey _isValidatingServerBrowserJoin _pendingValidatedServerBrowserLobby _pendingValidatedPasswordAttempt 
- methods: Awake(); ChangeFromLobbyToGame(); CheckForColdStartLobbyJoin(); ClearAchievement(string achievementID); ClearPendingServerBrowserValidation(); ColdStartJoinAfterDelay(); DelayedNameUpdate(float delay); DelayedUpdateLobbyPlayerCountData(float delay); DoesLobbyStillHaveOriginalHost(CSteamID lobbyId); DoesServerBrowserPasswordMatch(string lobbyId, string passwordAttempt); EstimateLobbyLatency(CSteamID lobbyId); FailEnteredServerBrowserLobby(string reason); FailServerBrowserJoinBecauseLobbyClosed(string reason); FusionReadyForLobbyJoins(); GetCurrentLobbyName(); GetHostInfo(); GetLobbyIntData(CSteamID lobbyId, string key, int fallback); GetLobbyMembers(); GetOriginalLobbyHost(CSteamID lobbyId); GetPlayerGamertag(); GetPlayerProfilePic(ProfilePicSize size = ProfilePicSize.Medium); GetPlayerUID(); GetPreferredFusionHostRegion(); GetSteamImageAsTexture2D(int iImage); GoToMenu(NetworkErrors _errorType); HandleEndSessionReturn(NetworkErrors _errorType); HideCurrentLobbyFromBrowserIfOwner(string reason); HostLobby(); IsEnterOrLeaveState(EChatMemberStateChange stateChange); IsLeaveState(EChatMemberStateChange stateChange); JoinPendingLobby(); JoinServerBrowserLobby(string lobbyId, string passwordAttempt); JoinTimeoutWatchdog(); JoinValidatedServerBrowserLobby(CSteamID lobbyId); OnGameLobbyJoinRequested(GameLobbyJoinRequested_t callback); OnLobbyChatUpdate(LobbyChatUpdate_t callback); OnLobbyCreated(LobbyCreated_t callback); OnLobbyDataUpdated(LobbyDataUpdate_t callback); OnLobbyEntered(LobbyEnter_t callback); OnLobbyMatchList(LobbyMatchList_t result, bool ioFailure); RefreshServerBrowserLobbies(); ServerBrowserJoinWatchdogRoutine(); SetLobbyRegion(string region); ShowErrorAfterSceneLoad(NetworkErrors _errorType); ShowFriendsUI(); StartNetworkMonitoring(); StartServerBrowserJoinWatchdog(); SteamError(string text); SteamLog(string text); UpdateLobbyPlayerCountData(); WaitForFrames(int frameCount); 

## class PlatformManager_Xbox : PlatformManager

## class PlayAudioArray : MonoBehaviour
- fields: maxPitch minPitch audios index hasDelayedEventOnPlay delayedEventOnPlay 
- methods: DelayedEventOnPlay(); PlayAudio(); 

## class PlayerLobbyHandler : NetworkBehaviour
- fields: __IsReady _prevIsReady readyButton nameText readyTick __CurrentPlayer _resolvedName _nameRetryTimer k_NameRetryInterval k_NameRetryTimeout _nameRetryElapsed _nameResolved _nameTimeoutCoroutine _NetworkedGamertag _prevNetworkedGamertag _NetworkedXUID _prevNetworkedXUID kickButton _kickButtonComponent kickedFromLobbyWarning 
- methods: ApplyGamertag(string tag); ClientConnected(); CopyBackingFieldsToState(bool A_1); CopyStateToBackingFields(); Despawned(NetworkRunner runner, bool hasState); KickThisPlayer(); NameTimeoutFallback(); OnReadyButtonClicked(); OnReadyStatusChanged(bool oldValue, bool newValue); Render(); RetrySetGamertag(); Rpc_KickedFromLobby(); Rpc_ToggleReady(); SetupKickButton(); Spawned(); 

## class PlayerManager : NetworkBehaviour
- fields: specCam spectatingCamRendered mainCam mainCamRendered duration newColor newFogDensity playerName nameText stamina maxStamina staminaBar1 staminaBar2 isRunning staminaPaused fpsScript inventoryMan crosshair cameraHolderAnim paused pauseMenu controllerMenu canPause disablePauseDuringFinaleChase dmgScreen lightDmgScreen damaged deathScreen camShake outsideAudio rainAudio tpPlayer dead _downed _pauseDownedTimer downedUI revivingUI downBar downCollider timeDetected detectionArrows detectionArrowSprites detectionIndicatorsUI detectedUI detectedBar completelyDetected insideVent headbobAnim enemiesList audioMixer detectionMeterFloat sfxFloat musicFloat localTimeSpentOutside timeSpentOutside needsEggWarning explosiveRemote explosiveRemotePress dontAllowLockCursor thirdPersonMan interactMan charController hitBlood deathBlood limbs cameraHolder spectatingCamera canvas howToPlayScreen curNpcScript finished stuckUI stuck stuckOnAKey scent lookingAtShelf lookingAtComputer huntLight weepingAngels timeUntilCanHeal huntParticles playerInput bloodRain blink blinkNoAudio blinkPistolShot night13ForestAmbience halfBlink smallBlink jumpscareGoal jumpscareGoal2 standardFootsteps forestFootsteps curFootstepsArray finaleSequenceSpectatingCamera deathValuesObj bloodRainUIOverlay _shelfRestockButtonCallouts kickPlayerMenuButton placingPoster _setupCoroutine freeingFromStuckIndicator aOn aOff dOn dOff maxAmountOfStucks curAmountOfStucks stuckBar1 stuckBar2 gottenUnstuck leftStick wiggleText spamText cantGetStuck customizablesMenu customizablesMenuOpen inside lookingAtAngle maxLookDistance angelRaycastMask angelLookStates justStoppedPauseTimer justStunned justUnstunned justFire justOffFire stunned onFire stunnedUI fireUI timeBeforeFireTick statusEffectMask timeOnFire maxHealth _health healthIndicatorAdjustment healthBar healthBarIndicator healthText healUI forestBehindSFX deathText inTakeDamageCooldown beingHelpedUI startedFinalCutscene alreadyVoted amountOfVotes amountToVoteText storyModeValues completingDay quitGameConfirmation inForest curChangeEnvLightingCoroutine _deathCameraCoroutine 
- methods: AddToEnemiesList(NetworkObject obj); Awake(); CanGetStuckAgain(); CancelActivePosterSession(); ChangeDeathText(string deathType); ChangeEnvironmentLighting(Color newColor, float newFogDensity, float duration); ChangeInsideStatus(bool on); ChangeInsideVent(bool change); ChangeMaxHealth(float newMaxHP); ChangeScent(float scent_); ChangeTimeDetected(float change, int index); ChangeTimeSpentOutside(float time); CheckIfAnyoneCanBeRevived(); CloseCustomizablesMenu(); CompleteDay(); ConvertStringToKeyCode(string keyName); CopyBackingFieldsToState(bool A_1); CopyStateToBackingFields(); DeathCameraTransition(); Die(); FinTakeDamageAnim(); FinishHowToPlay(); FinishTakeDamageCooldown(); FixedUpdate(); ForceUnpauseForSceneTransition(); FurtherSetupHowToPlay(); GetDamageDirection(Vector3 enemyPosition); GetStuck(float amountOfStucks); GetUnstuck(); Heal(float health_, bool playHealUI = true); HealToMax(); HideShelfPrompts(); IsNetworkBehaviourValid(NetworkBehaviour nb); LoadLatestSave(); LogAllNetworkBehaviours(); OnControlsChanged(PlayerInput input); OnTriggerEnter(Collider other); OnTriggerExit(Collider other); OpenCustomizablesMenu(); PauseDownedTimer(bool change); PlayFootstep(); PlayerLeft(PlayerRef player); RecoverDamage(); Render(); RequestReload(); ResetFinalSequence(); Respawn(); RespawnCameraTransition(); Revive(); Rpc_AddToEnemiesList(NetworkId netId); Rpc_CMD_AddToEnemiesList(NetworkId netId); Rpc_CMD_ChangeInsideStatus(bool on); Rpc_CMD_ChangeInsideVent(bool change); Rpc_CMD_ChangeMaxHealth(float newMaxHP); Rpc_CMD_ChangeScent(float s); Rpc_CMD_ChangeTimeDetected(float change, int index); Rpc_CMD_ChangeTimeSpentOutside(float time); Rpc_CMD_CompleteDay(); Rpc_CMD_Die(); Rpc_CMD_Downed(); Rpc_CMD_GetStuck(float amountOfStucks); Rpc_CMD_Heal(float amount); Rpc_CMD_LoadLatestSave(); Rpc_CMD_PauseDownedTimer(bool change); Rpc_CMD_ResetFinalSequence(); Rpc_CMD_Respawn(); Rpc_CMD_Revive(); Rpc_CMD_SetPlayerName(string name); Rpc_CMD_TakeDamage(float damage, bool significantAnim, string type); Rpc_CMD_TurnOffObjectsChild(NetworkObject obj); Rpc_ChangeHuntLight(bool on); Rpc_ChangeInsideStatus(bool on); Rpc_ChangeInsideVent(bool change); Rpc_ChangeMaxHealth(float newMaxHP); Rpc_ChangeScent(float scent_); Rpc_ChangeTimeDetected(float change, int index); Rpc_ChangeTimeSpentOutside(float time); Rpc_CompleteDay(); Rpc_Die(); Rpc_Downed(); Rpc_GetStuck(float amountOfStucks); Rpc_ResetFinalSequence(); Rpc_Respawn(); Rpc_Revive(); Rpc_SetPlayerName(string name); Rpc_SetPoster(CreatingPoster poster); Rpc_TakeDamage(float damage, bool significantAnim, string type); Rpc_TurnOffObjectsChild(NetworkObject obj); SetActiveWithParents(GameObject obj, bool active = true); SetHealthUI(); SetPauseFalse(); SetUpHowToPlay(); SpawnFlesh(); Spawned(); Start(); SwitchStuckKeys(); SwitchToAnotherPlayerSpectatingCamera(); TakeDamage(float damage, bool significantAnim, string type); TakeDamageAnim(); TurnOffAllDetectionArrows(); TurnOffObjectsChild(NetworkObject obj); TurnPauseBackOn(); TurnPauseOff(); UnlockMove(); Unpause(); UnpauseStamina(); Update(); WaitForStoreManagerThenSetup(); 

## class PlayerReady : NetworkBehaviour
- fields: _isReady 
- methods: CopyBackingFieldsToState(bool A_1); CopyStateToBackingFields(); Rpc_CMD_SetReady(bool ready); 

## class PopulationSystemManager : MonoBehaviour
- fields: planePrefab circlePrefab pointPrefab isConcert isStreet mousePos 
- methods: Concert(Vector3 pos); Street(Vector3 pos); 

## class PurchaseManager : NetworkBehaviour
- fields: balanceText purchaseQueue purchaseObjects cartHolder purchaseSfx eodBus fakeEodBus infoText refreshesText decorNodes storeUpgradeNodes weaponNodesUnlimited weaponNodesLimited crateNode nodePositions existingNodes noMoreStoreUpgrades noMoreWeapons refreshBTN itemsPurchasedThisRefresh refreshTip selectionFocusWhitelist isShowingShop timeBetweenRevealingCards prevStoreUpgradeIndex prevWeaponIndex prevWeaponIndex2 shopTabBTN shopTabNotifs activated 
- methods: ActivateShopTab(); ActivateShopTabInitial(); ActuallyDeleteAllNodes(); AddRefreshes(int change); Awake(); CopyBackingFieldsToState(bool A_1); CopyStateToBackingFields(); GeneratePurchaseNodeNavigation(); GenerateShopNodes(); GenerateShopNodesCoroutine(); GetRandomAvailableIndex(int maxExclusive, string type); HoverInfo(string item); LoadTotalBalance(); PurchaseItem(int index, float cost); Rpc_ActivateShopTab(); Rpc_ActivateShopTabInitial(); Rpc_BuyPlayerMaxHealthRpc(float hp); Rpc_CMD_ActivateShopTab(); Rpc_CMD_ActivateShopTabInitial(); Rpc_CMD_AddRefreshes(int change); Rpc_CMD_PurchaseObj(int index, float cost); Rpc_CMD_Refresh(); Rpc_EnableRefreshBTN(); Rpc_PurchaseObj(int index, float cost); Rpc_Refresh(); Rpc_SetRefreshesForAllClients(int refreshes); SelectButtonForControllerNavigation(); SetShopShowStatus(bool isShowing); TryRefresh(); UnhoverInfo(); Update(); WaitForNodesThenNavigate(); 

## class PurchaseNode : NetworkBehaviour
- fields: normalCost costVariation purchased purchaseBTN cost priceText isUpgradeNode isWeaponNode nodeIndex 
- methods: AttachToFirstAvailableNode(); CopyBackingFieldsToState(bool A_1); CopyStateToBackingFields(); Despawned(NetworkRunner runner, bool hasState); HoverInfo(string item); PurchaseItem(int index); Rpc_CMD_AskForCost(); Rpc_CMD_PurchaseItem(); Rpc_PurchaseItem(); Rpc_SetCost(float cost_); Spawned(); Start(); ToDollarString(float value); UnhoverInfo(); 

## class QuestionCooldown : MonoBehaviour
- fields: questionCooldownHolder questionCooldownBar storeMan justTurnedOff 
- methods: FixedUpdate(); Start(); 

## class QuitPopupButtonFocus : MonoBehaviour
- fields: whiteList 
- methods: Update(); 

## class RandomEvent : ScriptableObject
- fields: id eventIndex oneTimeEvent onlyOccurAfterThisDay onlyOccurBeforeThisDay 

## class RatCountdown : MonoBehaviour
- fields: curRats maxRats ratsAmount timeRemaining secondsRemaining gotObjective 
- methods: Awake(); FixedUpdate(); FormatTime(int totalSeconds); GotARat(); StartEvent(int maxRats_); 

## class Recoil : MonoBehaviour
- fields: recoilHolder gunPivot playerCamera recoil initialSmoothTime settleSmoothTime originalRotation targetRotation currentRotation targetVelocity currentVelocity kickback tilt kickbackInitialSmoothTime kickbackSettleSmoothTime gunCurrentPos gunTargetPos gunDefaultPos gunCurrentTilt gunTargetTilt gunTargetVelocity gunCurrentVelocity gunTargetTiltVelocity gunTurrentTiltVelocity 
- methods: GenerateRecoil(); GenerateSmallRecoil(); Start(); Update(); 

## class Register : Interactable
- fields: bellSFX canCompleteTransaction dentistTransaction 
- methods: CopyBackingFieldsToState(bool A_1); CopyStateToBackingFields(); Interact(PlayerManager playerMan); Rpc_Interact(PlayerRef player); 

## class RegularBrowsingNPC : NetworkBehaviour
- fields: matFaders target speed seeker pathfinder hittable generatedGoals index anim idleAnim walkAnim runAnim fastRunAnim damageAnim deathAnim pooingAnim curAnim takenDamage dropBloodParticles hitParticles runSpeed leaving justGoingToBathroom bathroomStallIndex pooParticle emotionIcon trashDropPoint trashPieces exitCoroutine startedFadingAway _NetworkedPosition _NetworkedRotation _IsPositionInitialised _hasSnappedToInitialPosition 
- methods: ChangeSpeed(float newSpeed); CopyBackingFieldsToState(bool A_1); CopyStateToBackingFields(); DelayedStart(); Die(); DropTrash(); FixedUpdateNetwork(); GoToBathroom(); GoToExit(); GoToNextShelf(); IdleAnim(); InvokeTookDamage(); OnTriggerEnter(Collider other); RegularAnim(); Render(); RevertToCurSpeed(); Rpc_CMD_ChangeSpeed(float newSpeed); Rpc_CMD_RevertToCurSpeed(); Rpc_CMD_Trigger(string triggerName); Rpc_ChangeSpeed(float newSpeed); Rpc_RevertToCurSpeed(); Rpc_Trigger(string triggerName); RunOutOfStore(); Spawned(); StartNextPathway(); StartRunning(); TookDamage(bool stunWhenHit = true); TriggerAnim(string triggerName); WalkOutOfStore(); 

## class RemoteTrap : NetworkBehaviour
- fields: pressEvent instantiateObj alreadyPressed _inventoryManager 
- methods: ActuallyDestroy(); CopyBackingFieldsToState(bool A_1); CopyStateToBackingFields(); DestroyAfterTime(float time); Explosion(float explosionRadius); InstantiateObj(); Press(); Rpc_CMD_Press(); Rpc_Press(); Rpc_SetRemoteTrap(NetworkObject trap); Spawned(); 

## class RemoveIfNotController : MonoBehaviour
- fields: playerInput gameobjects 
- methods: Awake(); OnEnable(); Update(); 

## class RemoveIfNotKeyboard : MonoBehaviour
- fields: playerInput gameobjects 
- methods: Awake(); FixedUpdate(); 

## class RemoveOnConsole : MonoBehaviour
- fields: destroyInsteadOfDisable removeOnSteamDeck removeOnXboxPC 
- methods: Awake(); DeactivateOrDestroy(); 

## class RemoveOnSteam : MonoBehaviour
- fields: destroyInsteadOfDisable 
- methods: Awake(); 

## class RemoveOnXbox : MonoBehaviour
- fields: destroyInsteadOfDisable 
- methods: Awake(); 

## class ResolutionDropdown : MonoBehaviour
- fields: dropdown preferLastSavedOverCurrent PrefKey _uniqueResolutions _options 
- methods: ApplyResolution(int index); Awake(); BuildUniqueResolutionList(); FindMatchIndex(int width, int height); GetStartupIndex(); Init(); OnChanged(int index); OnDestroy(); ResetToDefault(); SetDropdownWithoutNotify(int index); Start(); 

## class RestockShelf : ConstrictedInteractable
- fields: newCam actualCamera interacting shelfMan exclamationMark _productsOnShelf maxProductsOnShelf removeAtStartAmount tutorialShelf playerPos playerInput finishingCutscene 
- methods: AutoUpdateBar(); Awake(); CheckIfFullTutorial(); CopyBackingFieldsToState(bool A_1); CopyStateToBackingFields(); FinishedStocking(); Interact(PlayerManager playerMan); RemoveAtStart(); RemoveSome(); Rpc_CMD_RecalculateProducts(); Rpc_CMD_SetPosition(Vector3 newPosition, NetworkObject playerObject); Rpc_CMD_StopInteract(NetworkObject playerObject); Start_(); StopInteract(); Update(); 

## class RevealText : MonoBehaviour
- fields: txt text key typingAudioArray justPlayed revealTextRoutine 
- methods: ChangeText(string txt); OnEnable(); Refresh(bool immediate = false); RevealText_(string txt); SetText(string txt); Start(); 

## class Review : MonoBehaviour
- fields: stars name_ reviewCount reviewDesc 

## class ReviewsManager : NetworkBehaviour
- fields: overallRating decorPoints hygienePenalty stockPenalty decorText hygieneText stockText starText starText_ starText__ recommendedDecorText decorBar hygieneBar stockBar decorBarImage hygieneBarImage stockBarImage green yellow red emotionSprites happySprite midDecorSprite angryDecorSprite midHygieneSprite angryHygieneSprite midStockSprite angryStockSprite reviewsTab reviewsTabNotif lowestVal lowestRating reviewHolder reviewObj 
- methods: Awake(); CheckReviewScoreHint(); CopyBackingFieldsToState(bool A_1); CopyStateToBackingFields(); CreateReview(int reviewIndex, bool perfectReview, string reviewLevel); EnforceMaxReviews(); GetReview(); GetTargDecorPoints(); Rpc_SpawnReview(int reviewerNameIndex, int reviewCountIndex, int reviewIndex, int stars_, bool perfectReview, string reviewLevel); Rpc_UpdateStockPenalty(float newStockPenalty); UpdateDecorPoints(int change); UpdateHygienePenalty(int change); UpdateReviewUI(); UpdateStockPenalty(int change); 

## class ReviewsTab : MonoBehaviour
- fields: eventTriggers playerInput currentIndex k_timeBetweenInputs timeTilNextInput k_deadzone 
- methods: Awake(); DecrementIndex(); IncrementIndex(); OnEnable(); SelectEventTrigger(); Update(); 

## class RoachCountdown : MonoBehaviour
- fields: curRats maxRats ratsAmount timeRemaining secondsRemaining gotObjective 
- methods: Awake(); FixedUpdate(); FormatTime(int totalSeconds); GotARoach(); OnRoachKilled(); StartEvent(int maxRats_); 

## class RopeBetweenTwoPoints : MonoBehaviour
- fields: start end solver fixedParticleCount numParticles 
- methods: Generate(); Start(); 

## class RopeNet : MonoBehaviour
- fields: material resolution size nodeSize 
- methods: Awake(); CreateNet(ObiSolver solver); CreateRope(Vector3 pointA, Vector3 pointB); PinRope(ObiRope rope, ObiCollider bodyA, ObiCollider bodyB); 

## class RopeSweepCut : MonoBehaviour
- fields: cam rope lineRenderer cutStartPosition cutEndPosition cut 
- methods: AddMouseLine(); Awake(); DeleteMouseLine(); LateUpdate(); OnDestroy(); OnDisable(); OnEnable(); ProcessInput(); Rope_OnBeginSimulation(ObiActor actor, float stepTime, float substepTime); ScreenSpaceCut(Vector2 lineStart, Vector2 lineEnd); SegmentSegmentIntersection(Vector2 A, Vector2 B, Vector2 C, Vector2 D, out float r, out float s); 

## class RopeTenser : MonoBehaviour
- fields: force 
- methods: Update(); 

## class RopeTensionColorizer : MonoBehaviour
- fields: minTension maxTension normalColor tensionColor tenser tenserThreshold tenserMax rope localMaterial 
- methods: Awake(); OnDestroy(); Update(); 

## class Rotateobject : MonoBehaviour
- fields: speed 
- methods: Start(); Update(); 

## class RunEventOnEnable : MonoBehaviour
- fields: eventOnEnable eventOnDisable 
- methods: OnDisable(); OnEnable(); 

## class RuntimePathVisualizer : MonoBehaviour
- fields: lineWidth seeker lr 
- methods: Awake(); OnDisable(); OnEnable(); OnPathComplete(Path p); 

## class SaveData : Il2CppSystem.Object
- fields: curDay curDifficulty money tokens spawnedBefore npcsKilled instantiablesID instantiablesShiftsAlive instantiablesAssociatedTexts instantiablesPosX instantiablesPosY instantiablesPosZ instantiablesRotX instantiablesRotY instantiablesRotZ instantiablesRotW seed dayObjsSpawnedBefore maxShelfItems curShelfItems maxInventorySpace refreshes storeUpgradesPurchased weaponsPurchased playerIds inventoryIds inventoryAmounts itemStorages itemStorages2 trashAmounts huntsDone customizablesUnlocked todayMoneyGained todayMoneyLost maxHealth carsSpawnedBefore personalFunds trapsPurchased 

## class SaveFileManager : MonoBehaviour
- fields: noSaveFileHolders saveFileExistsHolders dayTexts moneyTexts modeTexts curSaveFileDeleting areYouSureYouWantToDelete curSaveFileCreating selectModeMenu lockedEndlessModeObj endlessModeButton activeSaveSlotUI backButton storyModeButton playerInput controllerPrompts selectedSaveSlot controllerOptionsShown currentSelectedID deleteSaveButton controllerPromptsPadding controllerPromptsPositions createLobbyScript nightNums 
- methods: ConfirmCreateNewSave(bool endlessMode); ConfirmDeleteSaveFile(); CreateNewSave(int index); DisableGameObjects(Il2CppReferenceArray<GameObject> objects); LoadAllSaveFileValues(); OnDeleteSave(InputAction.CallbackContext context); OnDisable(); OnEnable(); OnSelectSave(InputAction.CallbackContext context); OnShowSaveOptions(InputAction.CallbackContext context); SelectSaveFile(int index); TryToDeleteSaveFile(int index); Update(); 

## class SaveInProgressAnimation : MonoBehaviour
- fields: icon 
- methods: OnDisable(); Update(); 

## class SaveManager : NetworkBehaviour
- fields: curDay curDifficulty money tokens spawnedBefore npcsKilled npcsKilledTemp instantiablesID instantiablesShiftsAlive instantiablesAssociatedTexts instantiablesPosX instantiablesPosY instantiablesPosZ instantiablesRotX instantiablesRotY instantiablesRotZ instantiablesRotW seed dayObjsSpawnedBefore carsSpawnedBefore storeUpgradesPurchased weaponsPurchased refreshes _maxInventorySpace maxShelfItems curShelfItems playerIds inventoryIds inventoryAmounts itemStorages itemStorages2 trashAmounts huntsDone instantiablesAtlas securityScanner aisleSigns ceilingFans mandatoryRevenue maxHealth customizablesUnlocked todayMoneyGained todayMoneyLost MAX_POSTERS trapsPurchased k_personalEarningsKeySuffix setValuesForClientsCooldown alreadySpawnedPetTonight weaponObjs alreadySpawnedWeaponObjToday weaponSpawns storeUpgradeObjs alreadySpawnedStoreUpgradeObjToday storeUpgradeSpawnPositions storeUpgradeSpawnObjs _hasLoadedSave 
- methods: Awake(); CopyBackingFieldsToState(bool A_1); CopyStateToBackingFields(); DelayedLoadSave(); FixedUpdate(); GetActivePlayerSaveSlot(); GetGameMode(int saveSlot); GetPet(int defaultValue); GetPetAliveEndlessMode(int saveSlot, int defaultValue = 0); GetPetDead(int saveSlot, int defaultValue = 0); GetPetDying(int saveSlot, int defaultValue = 0); LoadSave(); NormalizeMultiList(ref List<List<int>> list, int expectedOuterCount, string name); Rpc_ActuallyGameplayValuesForClients(float money_, int tokens_, float maxHealth_, Il2CppStringArray npcsKilled_, int maxInventorySpace_, Il2CppStringArray playerIds_, float quota, Il2CppStructArray<int> customizablesUnlocked_, int personalFunds, int curDay_); Rpc_ActuallyItemStorageValuesForClients(Il2CppStructArray<int> itemStorages_Inner, Il2CppStructArray<int> itemStorages_Outer, Il2CppStructArray<int> itemStorages2_Inner, Il2CppStructArray<int> itemStorages2_Outer); Rpc_ActuallySetInventoryValuesForClients(Il2CppStructArray<int> inventoryIds_Inner, Il2CppStructArray<int> inventoryIds_Outer, Il2CppStructArray<int> inventoryAmounts_Inner, Il2CppStructArray<int> inventoryAmounts_Outer); Rpc_ActuallySetTrashValuesForClients(Il2CppStructArray<int> trashAmounts_Inner, Il2CppStructArray<int> trashAmounts_Outer); Rpc_CMD_SetStoreUpgradesAndWeaponsArsenal(); Rpc_CMD_SetValuesForClients(); Rpc_GetLockedAndLoaded(); Rpc_PurchasePet(); Rpc_SetPetDying(); Rpc_SetStoreUpgradesAndWeaponsArsenal(); Rpc_SetStoreUpgradesForClients(int objIndex); Rpc_SetValuesForClients(); Save(); SetEventQueuedState(int saveSlot, int eventId, int value, bool savePlayerPrefs = false); SetGameMode(int saveSlot, int gameMode); SetPet(int value); SetPetAliveEndlessMode(int saveSlot, int value); SetPetDead(int saveSlot, int val); SetPetDying(int saveSlot, int val); SetStoreUpgradesAndWeaponsArsenal(); SetValuesForClients(); Spawned(); Start(); UpdateStats(); WaitForArraysThenSetArsenal(); WaitForClientPlayer(); 

## class SaveSlotID : MonoBehaviour
- fields: _slotID 

## class SaveSnapshotObject : NetworkBehaviour
- fields: instantiableID dontKeepOnNewDay associatedString shiftsAlive maxShiftsAlive 
- methods: CheckShiftsAlive(int shiftsAlive_); CopyBackingFieldsToState(bool A_1); CopyStateToBackingFields(); 

## class SaveSystem : Il2CppSystem.Object
- methods: LoadState(); ResetSave(int saveSlot); SavePlayerPrefs(); SaveState(SaveManager saveMan); TryGetDayAndMoney(int saveSlot, out int day, out float money); 

## class ScanItem : Interactable
- fields: collectAnimator objectIndex col cost 
- methods: CopyBackingFieldsToState(bool A_1); CopyStateToBackingFields(); Interact(PlayerManager playerMan); Rpc_CMD_Interact(); Rpc_Interact(); 

## class ScanOnlyObject : MonoBehaviour
- fields: targetCollider boundsPadding 
- methods: InvokeUpd(); OnDisable(); OnEnable(); UpdateAroundTarget(); 

## class ScaryVentGirl : NetworkBehaviour
- fields: anim someoneHoldingGun explosiveArmed 
- methods: Awake(); ChangeHoldingGunStatus(bool x); CheckWhetherToHide(); CopyBackingFieldsToState(bool A_1); CopyStateToBackingFields(); Rpc_CMD_ChangeHoldingGunStatus(bool x); Rpc_ChangeHoldingGunStatus(bool x); 

## sealed class SceneVoiceAnchorManager : MonoBehaviour
- fields: voiceAnchorPrefab enableDebugLogs _runner _callbacksRegistered _spawnRoutine _anchorsByPlayer 
- methods: Awake(); BeginEnsuringVoiceAnchors(); DespawnAllVoiceAnchors(); DespawnVoiceAnchorForPlayer(PlayerRef player); EnsureVoiceAnchorsRoutine(); Log(string message); OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason); OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, Il2CppStructArray<byte> token); OnConnectedToServer(NetworkRunner runner); OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, Il2CppSystem.Object> data); OnDisable(); OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason); OnEnable(); OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken); OnInput(NetworkRunner runner, NetworkInput input); OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input); OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player); OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player); OnPlayerJoined(NetworkRunner runner, PlayerRef player); OnPlayerLeft(NetworkRunner runner, PlayerRef player); OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress); OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, Il2CppSystem.ArraySegment<byte> data); OnSceneLoadDone(NetworkRunner runner); OnSceneLoadStart(NetworkRunner runner); OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList); OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason); OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message); RemoveInvalidAnchorEntries(); RemoveRunnerCallbacks(); SpawnMissingVoiceAnchors(); SpawnVoiceAnchorForPlayer(PlayerRef player); Start(); TryResolveRunnerAndRegister(); ValidatePrefab(); 

## class ScrollRectControllerFocus : MonoBehaviour
- fields: _scrollViews _activeIndex 
- methods: OnDisable(); PopScrollRect(); PushScrollRect(ScrollRectControllerSupport obj); 

## class ScrollRectControllerSupport : MonoBehaviour
- fields: speed scrollRect playerInput scrollAction 
- methods: OnEnable(); OnScrollPerformed(); Update(); 

## class ScrollRectElementSelectHandler : MonoBehaviour
- fields: playerInput mouseInUse elementFocus 
- methods: Awake(); OnEnable(); OnSelect(BaseEventData eventData); Update(); 

## class ScrollRectFollowSelection : MonoBehaviour
- fields: scrollRect lastSelectedObject 
- methods: Awake(); LateUpdate(); ScrollSelectedIntoView(RectTransform selectedRect); 

## class ScrollTexture : MonoBehaviour
- fields: overrideMaterial objectsToScroll scrollSpeedX scrollSpeedY materials offsetX offsetY 
- methods: OnEnable(); ScrollRoutine(); Start(); 

## class ScrollViewElementFocus : MonoBehaviour
- fields: _scrollRect _centreElement 
- methods: ScrollToChild(RectTransform child); 

## class SearchResultButton : MonoBehaviour
- fields: nameText statusText dbName descriptionButton descIndex 
- methods: ClickButton(); 

## class SelectObjectOnDisable : MonoBehaviour
- fields: objToSelect 
- methods: OnDisable(); 

## class SelectOnEnable : MonoBehaviour
- fields: buttonPrompt 
- methods: OnEnable(); 

## class SelectPet : MonoBehaviour
- fields: playerInput _arrow _jail _petImage _happyPet _sadPet 
- methods: Awake(); DeselectIfSelected(); OnDeselect(BaseEventData eventData); OnSelect(BaseEventData data); Update(); 

## class SelectableGamertag : MonoBehaviour
- fields: button text playerId 
- methods: Init(ulong _playerId, string username); OnDisable(); OnPressed(); 

## static class SelectableUtils : Il2CppSystem.Object
- methods: RefreshVerticalNavigation(this Transform parentTransform); 

## class ServerBrowserLobbyObj : MonoBehaviour
- fields: lobbyId lobbyNameText passwordProtectedLobbyIcon globalLobbyIcon nightText gamemodeText playerCountText latencyText goodLatency midLatency badLatency lobbyName passwordProtected gamemode night playerCount latency maxPlayerCount 
- methods: TryEnterLobby(); UpdateLobbyUI(string lobbyId_, string lobbyName_, bool passwordProtected_, int gamemode_, int night_, int playerCount_, int latency_, int maxPlayerCount_ = 3); 

## class ServerBrowserManager : MonoBehaviour
- fields: lobbies showPasswordProtected showEndlessMode showStoryMode showPostStoryMode showFullLobbies onlyShowNight1 maxDisplayedLobbies passwordSubmitScreen incorrectPasswordPrompt passwordInputField confirmJoinScreen confirmLobbyNameText lobbyHolder lobbyObj fullLobbyWarning lobbyClosedWarning lobbyConsoleView pendingLobbyId pendingLobbyName pendingLobbyPasswordProtected useFakeLobbiesForTesting 
- methods: AddFakeLobbiesForTesting(); Awake(); ChangeOnlyShowNight1(bool on); ChangeShowEndlessMode(bool on); ChangeShowFullLobbies(bool on); ChangeShowPasswordProtected(bool on); ChangeShowPostStoryMode(bool on); ChangeShowStoryMode(bool on); ClearLobbyObjects(); EnterLobby(); OnDisable(); OnEnable(); OnServerBrowserLobbiesUpdated(List<BrowserLobbyData> foundLobbies); OnServerBrowserLobbyNoLongerExists(); RefreshLobbyList(); ShowConfirmJoinScreen(string lobbyName); ShowIncorrectPassword(); ShowLobbyNoLongerExistsAlert(); SubmitPassword(); TryEnterLobby(bool passwordProtected, string lobbyId, string lobbyName); TryToJoinFullLobby(); UpdateFilters(); 

## class SetInputMapOnEnable : MonoBehaviour
- fields: playerInput actionMap 
- methods: Start(); 

## class SettingsBackButtonNavigation : MonoBehaviour
- fields: buttonUnderBack lobbyBackButton menuBackButton 
- methods: SetBackButtonToLobby(); SetBackButtonToMenu(); 

## class SettingsInitializer : MonoBehaviour
- methods: Awake(); 

## class ShelfItemManager : NetworkBehaviour
- fields: outline productObj _amountOfProducts products halfLength zHalfLength itemIndex shelfMan 
- methods: AutoSort(); CopyBackingFieldsToState(bool A_1); CopyStateToBackingFields(); GetResidualOffsets(float halfLength, int residualCount); Hover(Vector3 startPos); LocalAddItem(bool autoSort); LocalReloadItems(); LocalRemoveItem(); Rpc_AddItem(bool autoSort); Rpc_CMD_AddItem(bool autoSort); Rpc_CMD_RemoveItem(); Rpc_ReloadItems(); Rpc_RemoveItem(); ServerAddItem(bool autoSort); ServerRemoveItem(); SortItems(); Start_(); Unhover(); 

## class ShelfManager : NetworkBehaviour
- fields: shelfItemManagers dragItemLayer dragPlaneLayer shelfItemLayer curOutline primaryPos secondaryPos productObjs camTarg completeSfx canvas initialQueueLength inventoryMan restockShelf boxAnim camTranslation amountText amountOfItems itemColliders mainProduct curDragItem curDragIndex hovering dragging inMenu cam justHovered lastShelfItem lastWrongShelfItem productsQueue finishBoxParticles fullAmountOfItemsInShelf dragTooltip dragTooltip_ playerInput currentSelectedItemBox k_moveInputCooldown currentCooldown numTopRowElements numMiddleRowElements numBottomRowElements k_stickDeadzone 
- methods: Awake(); CompleteItem(); CopyBackingFieldsToState(bool A_1); CopyStateToBackingFields(); DragItem(); InitObjectQueue(); MoveCamToPosition(); RecalculateProductsOnShelf(); RemoveRandomItems(int amountOfItems); ShootRay(); Start(); SwapProductPos(); Update(); 

## class Shelves : MonoBehaviour
- fields: restockShelves 
- methods: Awake(); 

## static class ShfuffleExtension : Il2CppSystem.Object
- fields: RandomGenerator 

## class ShirtColourManager : MonoBehaviour
- fields: pickerBTN pickingMenu pickImages 
- methods: ClickPickerBTN(); PickColor(int color); ShowPickingMenu(bool show); Start(); 

## class Shrine : NetworkBehaviour
- fields: fillAmount fillUpObjs waterDropObjs cutsceneObj filledBucket filledBucketSpawnPoint antlerText afterShrineTokens 
- methods: CopyBackingFieldsToState(bool A_1); CopyStateToBackingFields(); EnableTokens(); PlayDialogue(); ResetPlayerState(); RevealText(); Rpc_CMD_FillUp(); Rpc_FillUp(); Start(); 

## class ShrineEvent : MonoBehaviour
- fields: existingFence tenAntlersHittable forestObjects 
- methods: ExploreForestObjective(); OnEnable(); Start(); 

## static class SimpleJsonCrypto : Il2CppSystem.Object
- fields: Key 
- methods: DecryptBase64ToJson(string base64); EncryptJsonToBase64(string json); 

## class SingleButtonHighlight : MonoBehaviour
- fields: playerInput previousFrameWasController 
- methods: Start(); Update(); 

## class SlowmoToggler : MonoBehaviour
- methods: Slowmo(bool slowmo); 

## class SmoothPivotRotation : MonoBehaviour
- fields: swaySpeed targetRotation emptyGameObject rotationGizmoTransform highestParent 
- methods: FixedUpdate(); Start(); 

## class SmoothScrollRect : MonoBehaviour
- fields: scrollRect smoothSpeed targetPosition 
- methods: ScrollTo(float normalizedPos); Start(); Update(); 

## class SnakeController : MonoBehaviour
- fields: headReferenceFrame headSpeed upSpeed slitherSpeed rope solver traction surfaceNormal 
- methods: AnalyzeContacts(Il2CppSystem.Object sender, ObiNativeContactList e); OnDestroy(); ResetSurfaceInfo(ObiActor a, float simulatedTime, float substepTime); Start(); 

## class SpeakSFXManager : MonoBehaviour
- fields: audios maxPitch minPitch 
- methods: PlaySFX(); 

## class SpeakerRemovedCallback : MonoBehaviour
- fields: _speaker _onRemoved 
- methods: OnDestroy(); Setup(Speaker speaker, Il2CppSystem.Action<Speaker> onRemoved); 

## class SpeakingManager : NetworkBehaviour
- fields: name values key value entries curId curKey curName curOnlyClientSide curDialogueScript curKeyIndex moreDialogueToScroll subtitleHolder subtitleText chatAudioArray playChatAudio stillScrollingText inChat chatLogImages chatQueue chatContent chatLogNode chatLogNodeHolder chatLogHolder dialogueScrollSpeed transactionManager ignoreClicks playerInput speakingCanv 
- methods: AddChatLogNode(string dialogueId, string key); AutoClickNext(); Awake(); CancelAllDialogue(); ClickNext(); CopyBackingFieldsToState(bool A_1); CopyStateToBackingFields(); DisableChat(); GetDialogueText(string id, string key, bool usesKeyIndex); GetDialogueText(string id, string key, bool usesKeyIndex, out string resolvedKey); InsertLineBreaks(string input, int interval = 50); NewDialogueBranch(string id, string key, string nameInDialogue, DialogueInteractable dialogueScript, bool onlyClientSide = false); NextDialogueBranch(); RevealTextUniversal(); Rpc_AddChatLogNode(string dialogueId, string key); Rpc_CMD_AddChatLogNode(string dialogueId, string key); SetText(string dialogueId, string key, bool clientSideOnly); Start(); TryGetDialogueEntryDict(string id, out Dictionary<string, string> dict); TurnOffInChat(); Update(); 

## class SpectatingCamera : MonoBehaviour
- fields: players curPlayerIndex 
- methods: OnEnable(); 

## class Spider : Enemy
- fields: level primaryCreature spiderSpawn patrolAreas huntMan seeker pathfinder normalSpeed runSpeed annoyedSpeed rampageSpeed timeAtPatrol curSpeed walkTarget barricadeTarget scentTarget playerScentTarget ventTarget chaseTarget checkingVent chasingTarget breakingBarricade justAttackedBarricade attackBarricadeCooldown waitingAtPatrolTime alreadyStartedNextPathway justStartedPatrol canAttack chasingNonPlayerObject attackAudio roarAudio interestedAudio justDetectedPlayer spiderAnim playerMans detectionOfEachPlayer timeChasingObject timeChasingBeforeGivingUp alreadyCheckedVent justFoundVent annoyed annoyedSFXObject leaveSFX beingHit oneCreature thisPlayer enemyHolder fastPacing headMaterial bodyMaterial chasingPlayer chasingPlayerTarg timeChasingPlayer shootingProj timeShootingProj silkEgg silkProjectile timeBetweenAttacks interstitialCobwebSpawnPoint interstitialCobweb legsObjs alreadyLeft shotProj justCheckedVents fastPacingButDontChasePlayer _NetworkedPosition _NetworkedRotation 
- methods: AnnoyedHint(); BecomeAnnoyed(); CanAttack(); ChangeSilkEgg(bool on); ChaseNonPlayerTarget(Vector3 targPosition); CheckEnemiesLeft(float timeUntilCheck); CheckIfNearBarricade(); CheckIfNearVent(); CheckVentCooldown(); CompleteHunt(); CopyBackingFieldsToState(bool A_1); CopyStateToBackingFields(); DetectPlayers(); Die(); DoneShootingProj(); FinishHit(); FixedUpdate(); FixedUpdateNetwork(); GoToBreakBarricade(); GoToCheckVent(); GoToPatrol(); GoToTarget(); Hit(); Leave(); Leaving(); PlaceInterstitialCobweb(); Render(); Rpc_Annoyed(); Rpc_CMD_ChangeSilkEgg(bool on); Rpc_CMD_ChaseNonPlayerTarget(Vector3 targPosition); Rpc_CMD_Die(); Rpc_CMD_Leave(); Rpc_ChangeHittableHealth(float health); Rpc_ChangeSilkEgg(bool on); Rpc_ChaseNonPlayerTarget(Vector3 targPosition); Rpc_Leave(); Rpc_PlayRoar(); ShootProj(); ShootingProj(); Spawned(); Start(); StartNextPathway(); UpdatePlayerLists(); 

## class Spill : NetworkBehaviour
- fields: spillParticle spillDecal col decalProj targScale firstTimeStart minScale maxScale cantClean xRot randomYRot randomZRot growBlood cleaned 
- methods: CopyBackingFieldsToState(bool A_1); CopyStateToBackingFields(); GrowBlood(); RequestClean(); Rpc_ActuallyClean(); Rpc_CMD_Clean(); ServerActuallyClean(); ShrinkBlood(); Start(); 

## class SpillWarning : MonoBehaviour
- fields: spillCam 
- methods: FindPlayer(); Start(); Update(); 

## class SpinWheelManager : NetworkBehaviour
- fields: spinDuration minRotations maxRotations spinning timer startY targetY wheel arrowTip wheelObjects wheelWinIndicator winParticle winSfx nothingEarnedObj coinRigidbody maxAngleOffsetDeg minForwardVelocity maxForwardVelocity hatEarnedHint hatEarnedText hatEarnedIcon hatSprites spinSfx timeSpinning tickTimer 
- methods: CompleteSpin(); CopyBackingFieldsToState(bool A_1); CopyStateToBackingFields(); PressButton(); Rpc_CMD_Spin(); Rpc_NoMoreHats(); Rpc_SpawnToken(); Rpc_Spin(); Rpc_SpinTarget(float targY); Rpc_UnlockRandomHat(int hat); UnlockRandomHat(); Update(); 

## class SpiralCurve : MonoBehaviour
- fields: radius radialStep heightStep points rotationalMass thickness 
- methods: Awake(); Generate(); 

## class SplashScreenScene : MonoBehaviour
- fields: vp k_loadPlayerPrefsDelay 
- methods: LoadScene(); OnDestroy(); OnPrepared(VideoPlayer source); PlayVid(); Start(); 

## class StandingPeopleConcert : MonoBehaviour
- fields: planePrefab circlePrefab surface planeSize peoplePrefabs spawnPoints target peopleCount isCircle circleDiametr showSurface SurfaceType par looking damping highToSpawn 
- methods: GetRealPeopleModelSize(); GetRealPlaneSize(); IsRandomPositionFree(Vector3 pos); OnDrawGizmos(); PopulateButton(); RandomCirclePosition(); RandomRectanglePosition(); RemoveButton(); SpawnCircleSurface(); SpawnPeople(int _peopleCount); SpawnRectangleSurface(); 

## class StandingPeopleStreet : MonoBehaviour
- fields: planePrefab circlePrefab surface planeSize peoplePrefabs spawnPoints peopleCount isCircle circleDiametr showSurface SurfaceType par highToSpawn 
- methods: GetRealPeopleModelSize(); GetRealPlaneSize(); IsRandomPositionFree(Vector3 pos, Vector3 helpPoint1, Vector3 helpPoint2); OnDrawGizmos(); PopulateButton(); RandomCirclePosition(); RandomRectanglePosition(); RemoveButton(); SpawnCircleSurface(); SpawnPeople(int _peopleCount); SpawnRectangleSurface(); 

## class StartCutscene : MonoBehaviour
- fields: titleText titleTextPosition firstNextButton petString pickedPetText1 pickedPetText2 pickedPetText pickedPetTextPosition pickPetScreen namePetScreen inputField petNameInputField setPetName nextButton4Holder nextButton6Holder finalCutsceneScreen namedParticles sadPet happyPet couchPet sickPet happyPetImage1 happyPetImage2 sleepyPetImage sickPetImage sickPetImage2 fadeOut inputFieldTMP buttonAfterName 
- methods: ActuallyEnterGame(); CheckInputField(); EnableButton4(); EnableButton6(); EnableFinalCutscene(); EnableFirstNextButton(); EnablePickPetScreen(); EnterGame(); GoToFinalCutscene(); InvokePickPetScreen(); MovePetTextUp(); MovePetUpCoroutine(); MoveTitleTextUp(); MoveTitleTextUpCoroutine(); NamePetScreen(); PickPet(int petIndex); SetPetName(); Start(); UpdateInputFieldFont(); 

## class SteamDeckInputField : MonoBehaviour
- fields: _inputField 
- methods: OnDestroy(); OnInputFieldSelected(string arg0); Start(); 

## class SteamManager : MonoBehaviour
- fields: s_EverInitialized s_instance m_bInitialized m_SteamAPIWarningMessageHook 
- methods: Awake(); InitOnPlayMode(); OnDestroy(); OnEnable(); SteamAPIDebugTextHook(int nSeverity, StringBuilder pchDebugText); Update(); 

## class StoreBrowseBehaviour : NetworkBehaviour
- fields: idDatabaseName killedIDString isDoppelganger countsAsCustomer patienceCanvas patienceBar waitingAtCounter hasPatience maxPatience curPatience inCar matFaders hasLeavingRemark target speed curSpeed seeker pathfinder hittable useShelfGoals goals generatedGoals index overrideDefaultTransactionItems transactionItems anim secondaryAnim idleAnim walkAnim grabAnim runAnim damageAnim deathAnim curAnim playerLookTarget head canNeverInteract dialogueInteractable takenDamage dropBloodParticles hitParticles runSpeed transactionCompleteEvent hasTransactionCompleteEvent forceTalkAtVeryStart addToReport leaving _wanderAroundAimlessly timeStoppedWhileWandering isThief hasStolenItems doesntGiveBackItems _NetworkedPosition _NetworkedRotation _IsPositionInitialised _hasSnappedToInitialPosition targPlayerMan damageToPlayer attacking exitCoroutine goBehindCheckout getStunnedWhenHitAtAll chasingPlayer startTransactionEvent targetDistanceFromRegister stopAtShelf targetDistanceFromShelf startedFadingAway getAchievementForKilling hasTransacted dontUpdatePosition 
- methods: Attack(); BreakFrontDoorBarricade(); ChangeHasStolenItems(bool change); ChangeSpeed(float newSpeed); CopyBackingFieldsToState(bool A_1); CopyStateToBackingFields(); DealDamageToPlayer(); DelayedStart(); DentistTransaction(); Die(); FindNearestPlayer(); FinishAttack(); FinishTransaction(); FinishedStartForceTalk(); FixedUpdate(); FixedUpdateNetwork(); GoToExit(); GoToNextShelf(); GoToPlayerForcedTalk(); GoToRegister(); IdleAnim(); InvokeTookDamage(); OnTriggerEnter(Collider other); RegularAnim(); Render(); RestartExitCoroutine(); RevertToCurSpeed(); Rpc_CMD_ChangeHasStolenItems(bool change); Rpc_CMD_ChangeSpeed(float newSpeed); Rpc_CMD_RevertToCurSpeed(); Rpc_CMD_Trigger(string triggerName); Rpc_CancelPatience(); Rpc_ChangeHasStolenItems(bool change); Rpc_ChangeSpeed(float newSpeed); Rpc_RevertToCurSpeed(); Rpc_StartPatience(float patience); Rpc_Trigger(string triggerName); RunToPlayer(); Spawned(); StartNextPathway(); StartRunning(); StopExitCoroutine(); TookDamage(bool stunWhenHit = true); TriggerAnim(string triggerName); TurnOffInteractable(); VanessaDeadEvent(); WalkAroundUntilTransactionIsFree(); 

## class StoreManager : NetworkBehaviour
- fields: charController playerForward throwForceX charController playerForward throwForceX demo _HatLocker npcSpawnPoints carSpawnPoint storeStats truckHolder versionText hintQueue hintText hintCanv huntUI storeUI hygieneText stockText hygieneText2 stockText2 ratingText ratingText2 ratingText3 ratingText4 ratingImage hygiene stock storeRating storePoints playerLastPos inHunt storeLights hazardLights offLights rakeLighting volumeBar micVolumeBar volume micVolume _localRecorder goBackInsideWarning audioMixer huntObj huntMan huntAmbientMusic huntChaseMusic beingChased doppelsLetThru diffusersActivated diffusersText playerMans diffusers hoverAudio huntExplanation huntExplanation2 secondsLeftText secondsLeft startBus ventOutlines explanationText tokenBalanceText tokenBalanceText2 purchaseTokenBTN allCoins vendingOutline bearTrapTemplate bearTrapTemplateRed landmineTemplate landmineTemplateRed stunMineTemplate stunMineTemplateRed explosiveTemplate explosiveTemplateRed posterTemplate pottedPlantTemplate pottedPlantTemplateRed waterCoolerTemplate waterCoolerTemplateRed basketRackTemplate basketRackTemplateRed atmTemplate atmTemplateRed mailboxTemplate mailboxTemplateRed trashCanTemplate trashCanTemplateRed bannerTemplate bannerTemplateRed floorMatTemplate floorMatTemplateRed sunglassesRackTemplate sunglassesRackTemplateRed booksTemplate booksTemplateRed bobbleHeadTemplate bobbleHeadTemplateRed burgerTemplate burgerTemplateRed plant1Template plant1TemplateRed plant2Template plant2TemplateRed plant3Template plant3TemplateRed plant4Template plant4TemplateRed robotTemplate robotTemplateRed boomboxTemplate boomboxTemplateRed gumballTemplate gumballTemplateRed clockTemplate clockTemplateRed ivyTemplate ivyTemplateRed stringLightsTemplate stringLightsTemplateRed painting1Template painting1TemplateRed painting2Template painting2TemplateRed painting3Template painting3TemplateRed deerTemplate deerTemplateRed forceSpawnQueue alreadyCompleted amountOfCompletions minusTimeObj minusTimeText timeChangeText quota showingRevenue actualRevenue revenueText addRevenueText minusRevenueText moneyAudioArray aimlessPatrolPoints canvas dialogueTutorialCanv questionCooldown maxQuestionCooldown dumpster dumpsterOutline dissonanceIds steamIds rToReload noMoreAmmo gasPumpOrigin standFurtherBackWarning flashlightsOnAmount mandatoryRevenueText huntTooltipText bathroomTargets flushes curBrowsingNpcIndex allBrowsingNpcs currentBrowsingNPCs allNuisanceNpcs currentNuisanceNPCs allowedToSpawnBrowsingNPCs activeCustomerCount forestRakes forestRakeSpawnPoints rakePrefab multiplayerToggleScripts finalSequence poster posterSpawnPos todayWasSetDayObj everyoneCompleted endOfDayFakeOut ifDieThenCompleteDay fadeOut alreadyStarted greenColor redColor goldColor alertText alertBG alertErrorAudio alertSuccessAudio alertGoldAudio alertObj trapHighlights forestGate openForestGates huntHappened eligibleForHuntAchievement alreadyEndedHuntToday objectiveCanvas objectiveText amountToLookAtObjectiveText alreadyDone frontDoorBarricade coin pickupObjs thrownObjs cctvCam globalVolume normalProfile 
- methods: ActivateVentOutlines(); AddDissonanceDictionary(string dissonanceId, string steamId); AddHint(string hintId); AllBrowsingNPCsRunAway(); AllBrowsingNPCsWalkAway(); Awake(); BreachCompensation(); ChangePlayerVisibility(bool change); ChangeRevenue(string text, float money); ChangeTokenBalance(int change); CheckAllRemoteTraps(); CheckForHunt(); CheckWhosCompletedDay(); CollectRatObjective(); CollectRoachObjective(); CompleteDay(); CopyBackingFieldsToState(bool A_1); CopyStateToBackingFields(); CountDownSeconds(); DeactivateVentOutlines(); DealNonLethalDamageToPlayer(); DestroyFrontBarricade(); DiffuserActivated(); DrankLemonade(); EODScene(); EndHunt(); EnterCutscene(bool disablePlayerMan = true); ExitCutscene(); FinishObjective(); FixedUpdate(); FlashlightToggled(int change); ForestHint(); FormatTime(int totalSeconds); HazardLightsHint(); HuntObjective(); InitializeForestRake(); LoadAllPlayerMansServer(); NewObjective(string id, string key); NextHint(); NoLongerEligibleForHuntAchievement(); NormalSpeed(); RPC_ResetCompletedDay(); RedoCoins(); RevealObjectiveText(); Rpc_AddDissonanceDictionary(string dissonanceId, string steamId); Rpc_AnotherPlayerCompleted(); Rpc_BackToPlayer(); Rpc_CMD_AddDissonanceDictionary(string dissonanceId, string steamId); Rpc_CMD_AnotherPlayerCompleted(); Rpc_CMD_ChangeRevenue(string text, float money); Rpc_CMD_ChangeTokenBalance(int change); Rpc_CMD_CheckAllRemoteTraps(); Rpc_CMD_EndHunt(); Rpc_CMD_ExitCutscene(); Rpc_CMD_FlashlightToggled(int change); Rpc_CMD_NetworkDropObject(int holdingIndex, Vector3 throwPosition, Quaternion rotation, int itemStorage, int itemStorage2, Vector3 playerForward, InventoryManager inventoryMan); Rpc_CMD_NetworkThrowObject(int holdingIndex, Vector3 throwPosition, Quaternion rotation, Vector3 playerForward, NetworkObject charController, float throwForceX); Rpc_CMD_RoachKilled(); Rpc_CMD_StartFinalSequence(); Rpc_CMD_StartHazardLights(); Rpc_CMD_ToggleMultiplayerObjects(); Rpc_ChangeRevenue(string text, float money); Rpc_ChangeTokenBalance(int change); Rpc_CheckAllRemoteTraps(); Rpc_CheckIfDowned(); Rpc_DestroyTutorialStuff(); Rpc_DisableDumpsterMonster(); Rpc_EnableForestRakeRpc(int index, float spd); Rpc_EndHunt(); Rpc_ExitCutscene(); Rpc_FlashlightToggled(int change); Rpc_RoachKilled(); Rpc_SendPlayerListToClients(); Rpc_SetTokenBalance(int tokens_); Rpc_StartFinalSequence(); Rpc_StartHazardLights(); Rpc_TeleportPlayerToStore(); Rpc_ToggleMultiplayerObjects(); Rpc_UpdateCurDayOnAllClients(int curDay_); Rpc_UpdateDissonanceDictionaryToClients(Il2CppStringArray dissonanceList, Il2CppStringArray steamList); ServerThrowObject(int holdingIndex, Vector3 throwPosition, Quaternion rotation, Vector3 playerForward, GameObject charController, float throwForceX); SetAlert(string key, string color); SpawnBrowsingNPC(); SpawnNuisanceNPC(); SpawnPoster(); SpawnRakeFromPos(int index); Spawned(); StartCountingDown(); StartHazardLights(); StartHazardLights2(); StartHunt(); StartHunt2(); StartHuntNoMatterWhat(); Start_(); ThiefCaught(); ToggleMultiplayerObjects(); TriggerNextEvent(); WaitForPlayersServer(); 

## class StoreTravelPoints : MonoBehaviour
- fields: targPoints checkoutPoint behindCheckoutPoint exitPoint forestRakeExitPoints 
- methods: Awake(); 

## class StoryModeManager : NetworkBehaviour
- fields: dayText personalFundsText curEventIndex allEventObjs showingMoney prevEvent eventsHolder customerReportHolder eodReport 
- methods: CallClyde(); ChangeMoney(int amount); CopyBackingFieldsToState(bool A_1); CopyStateToBackingFields(); FixedUpdate(); InstantLoadStats(); LoadNextEvent(); LoadStatsNextFrame(); LoseGame(); QueueEvent(int eventIndex); Rpc_CMD_InstantLoadStats(); Rpc_InstantLoadStats(int personalFunds); Rpc_LoadCustomerReport(); Rpc_LoadEvent(int i); SetPetDead(); SetPetDying(); SetPetNotDying(); Spawned(); 

## class StoryScene : MonoBehaviour
- fields: titleText titleTextPosition firstNextButton petString pickedPetText1 pickedPetText2 pickedPetText pickedPetTextPosition pickPetScreen namePetScreen inputField petNameInputField setPetName nextButton4Holder nextButton6Holder finalCutsceneScreen namedParticles sadPet happyPet couchPet sickPet happyPetImage1 happyPetImage2 sleepyPetImage sickPetImage sickPetImage2 
- methods: EnableButton4(); EnableButton6(); EnableFinalCutscene(); EnableFirstNextButton(); EnablePickPetScreen(); GoToFinalCutscene(); InvokePickPetScreen(); MovePetTextUp(); MovePetUpCoroutine(); MoveTitleTextUp(); MoveTitleTextUpCoroutine(); NamePetScreen(); OnDestroy(); PickPet(int petIndex); SetPetName(); Start(); 

## class StorySpinWheel : NetworkBehaviour
- fields: successEvent failEvent anim successObj failObj triggerName 
- methods: CopyBackingFieldsToState(bool A_1); CopyStateToBackingFields(); Fail(); ReInvokeTrigger(); Rpc_UpdateResultForAll(bool result, int spinWheelAnimIndex); Start(); Success(); 

## static class StreamingAssetsReader : Il2CppSystem.Object
- fields: path onDone 
- methods: ReadTextAsync(string path, Il2CppSystem.Action<bool, string> onDone); TryReadTextSync(string path, out string text); 

## class StunMine : NetworkBehaviour
- fields: trapAnim monsterCheckScript caught cantPlaceRadius curCantPlaceRadius radiusPos interactable stunWave beep 
- methods: CopyBackingFieldsToState(bool A_1); CopyStateToBackingFields(); DisableRadius(); EnableRadius(); Rpc_CMD_Trap(); Rpc_Trap(); SpawnExplosion(); Start(); Trap(); 

## class TMPForceSingleLine : MonoBehaviour
- fields: inputField isSanitising 
- methods: Awake(); OnDisable(); OnEnable(); SanitiseText(string text); ValidateCharacter(string currentText, int characterIndex, char addedCharacter); 

## class TangledPeg : MonoBehaviour
- fields: slot currentSlot floorCollider attachedRope stiffness damping maxAccel minDistance 
- methods: Awake(); DockInSlot(TangledPegSlot slot); MoveTowards(Vector3 position); MoveTowardsSlot(TangledPegSlot slot); UndockFromCurrentSlot(); 

## class TangledPegSlot : MonoBehaviour
- fields: currentPeg tintColor instance normalColor 
- methods: Awake(); OnDestroy(); ResetColor(); Tint(); 

## class TangledRopesGameController : MonoBehaviour
- fields: pegSlots pegHoverHeight maxPegDistanceFromSlot framesWithoutContactsToWin onFinish selectedPeg floor framesSinceLastContact 
- methods: FindCandidateSlot(TangledPeg peg); OnDisable(); OnEnable(); Solver_OnParticleCollision(ObiSolver s, ObiNativeContactList e); Update(); 

## class Telephone : Interactable
- fields: whosCalling brokenDownCar outline ratInfestationCountdownCanvas ratInfestationCamera roachInfestationCountdownCanvas roachInfestationCamera corpseNightCamera telephoneDone curChatIndex ratCountdown roachCountdown carBrokenDown forestLimbsEvent shrineEvent interactedToday talkAudioArray suspiciousCustomer1 suspiciousCustomer2 indexWhereYouRespond ventAnims rat roach roachSpawnPoints roachSpawnIndex 
- methods: Awake(); CopyBackingFieldsToState(bool A_1); CopyStateToBackingFields(); ForestLimbsEvent(); GivePlayerControlAgain(); Interact(PlayerManager playerMan); Rpc_ActuallyInteract(PlayerManager playerMan); Rpc_ActuallyPlayTelephoneChatterAudio(); Rpc_CMD_CarBrokenDownEvent(PlayerManager playerMan); Rpc_CMD_CompleteOcc(); Rpc_CMD_CorpseNightEvent(); Rpc_CMD_ForestLimbsEvent(PlayerManager playerMan); Rpc_CMD_Interact(PlayerManager playerMan); Rpc_CMD_PlayTelephoneChatterAudio(); Rpc_CMD_RatInfestationEvent(); Rpc_CMD_RoachInfestationEvent(); Rpc_CMD_ShrineEvent(PlayerManager playerMan); Rpc_CMD_SuspiciousCustomerEvent(); Rpc_CarBrokenDownEvent(PlayerManager playerMan); Rpc_CompleteOcc(); Rpc_CorpseNightEvent(); Rpc_ForestLimbsEvent(PlayerManager playerMan); Rpc_RatInfestationEvent(); Rpc_RoachInfestationEvent(); Rpc_ShrineEvent(PlayerManager playerMan); Rpc_SuspiciousCustomerEvent(); RunDialogue(); ShrineEvent(); SpawnRat(); SpawnRoach(); TurnOnNextCanvas(); TurnOnRatCanvas(); TurnOnRoachCanvas(); 

## class TeleportPlayer : NetworkBehaviour
- fields: cc fpsScript pendingTeleport pendingTeleportPos pendingTeleportRot 
- methods: Awake(); CacheComponents(); CopyBackingFieldsToState(bool A_1); CopyStateToBackingFields(); RequestTeleport(Vector3 targetPos, Quaternion rot); Rpc_CMD_Teleport(Vector3 targetPos, Quaternion rot, RpcInfo info = default(RpcInfo); Rpc_Teleport(Vector3 targetPos, Quaternion rot); Spawned(); TeleportInternal(Vector3 targetPos, Quaternion rot); Update(); 

## class ThirdPersonCam : MonoBehaviour
- fields: cameraCollision hit linecastTarget target mouseX mouseY zOffset scrollSpeed yMin yMax 
- methods: NormalCamControl(); Update(); 

## class ThirdPersonManager : NetworkBehaviour
- fields: bodyAnim legsAnim armsAnim models models_ objsToTurnOffLocally objs spineRots camRot gunshotAnims flashlightLight clientPlayer mopAnim playerMan shirt shirtColors moodReading stuckObj unstuckParticles duckSqueak thirdPersonGasPump gasPumpParticles gasPumpParticlesOn stunnedObj fireObj chainsawAnim chainsawScrollTexture usingChainsaw nameCanvas flamethrowerFlame flamethrowerFlamePlaying hoseSfx hoseParticle hoseParticlePlaying upDownLookRot updateLookRotRate curUpdateLookRotRate hatObjs 
- methods: ApplySpineRotation(float lookRotX); ChangeFire(bool on); ChangeFlamethrowerFire(bool on); ChangeGasPumpParticles(bool on); ChangeHoseParticle(bool on); ChangeStunned(bool on); CopyBackingFieldsToState(bool A_1); CopyStateToBackingFields(); DropObj(); EquipObj(int item); FixedUpdate(); GetStuck(); GetUnstuck(); MeleeAttack(); NormalizeAngle(float angle); ResetAnims(); Rpc_CMD_ChangeFire(bool on); Rpc_CMD_ChangeFlamethrowerFire(bool on); Rpc_CMD_ChangeGasPumpParticles(bool on); Rpc_CMD_ChangeHoseParticle(bool on); Rpc_CMD_ChangeStunned(bool on); Rpc_CMD_ChangeVisibility(bool on); Rpc_CMD_DropObj(); Rpc_CMD_EquipObj(int item); Rpc_CMD_MeleeAttack(); Rpc_CMD_ResetAnims(); Rpc_CMD_SetChainsaw(bool on); Rpc_CMD_SetColor(int colorIndex); Rpc_CMD_SetMood(int x); Rpc_CMD_SetStuckVisual(bool on); Rpc_CMD_ShootGun(); Rpc_CMD_SqueakDuck(); Rpc_CMD_ToggleFlashlight(bool on); Rpc_CMD_ToggleMop(bool on); Rpc_CMD_UpdateHat(int index); Rpc_CMD_UpdateLookRot(float upDownLookRot_); Rpc_ChangeFire(bool on); Rpc_ChangeFlamethrowerFire(bool on); Rpc_ChangeGasPumpParticles(bool on); Rpc_ChangeHoseParticle(bool on); Rpc_ChangeStunned(bool on); Rpc_ChangeVisibility(bool on); Rpc_DropObj(); Rpc_EquipObj(int item); Rpc_MeleeAttack(); Rpc_ResetAnims(); Rpc_SetChainsaw(bool on); Rpc_SetColor(int colorIndex); Rpc_SetMood(int x); Rpc_SetStuckVisual(bool on); Rpc_ShootGun(); Rpc_SqueakDuck(); Rpc_ToggleFlashlight(bool on); Rpc_ToggleMop(bool on); Rpc_UpdateHatForOthers(int index); Rpc_UpdateLookRot(float upDownLookRot_); SetColor(); SetMood(); ShootGun(); SqueakDuck(); Start(); StartChainsaw(); StopChainsaw(); ToggleFlashlight(bool on); ToggleMop(bool on); TurnOffArmsAnim(); TurnOffModels(); UpdateHat(); 

## class ThrobbingAnimation : MonoBehaviour
- fields: startSize biggestSize goingBigger 
- methods: ChangeGrowth(); FixedUpdate(); Start(); 

## class ThrownObject : NetworkBehaviour
- fields: netObj gfx breakParticles rb monsterCheckEvent damage damageWhenHitPlayer dealDamageToPlayer alreadyHitSomething makesPlayerStuck spawnsExtraObjectOnHit extraObjectToSpawnOnHit canHitPlayersCollider dontDestroyOnHit playerManThrowing significantDamageAnim initialHitSomethingEvent onlyDamageOnce hitSomething onlyServerCanHandleHits ThrowingPlayerNetObj setThrowingPlayerCoroutine timeBeforeEnableCanHitPlayersCollider 
- methods: CopyBackingFieldsToState(bool A_1); CopyStateToBackingFields(); DeleteSelf(); EnableCanHitPlayersCollider(); MolotovHit(); OnCollisionEnter(UnityEngine.Collision collision); Rpc_CMD_SetThrowingPlayer(NetworkObject netObj); Rpc_CMD_SpawnObj(Vector3 pos, Quaternion rot); Rpc_CMD_ThrowComplete(); Rpc_InitialHitSomethingEvent(); Rpc_SetThrowingPlayer(NetworkObject netObj); Rpc_ThrowComplete(); SetThrowingPlayer(NetworkObject netObj); SetThrowingPlayerCoroutine(NetworkObject netObj); Update(); 

## class ToggleInMultiplayer : MonoBehaviour
- fields: multiplayerObject 
- methods: SetMultiplayer(); 

## class TransactionManager : NetworkBehaviour
- fields: card registerScript idCard itemsToScan itemAtlas itemPositions amountOfItemsToScan amountOfItemsScanned bell revenue registerText curNpcScript objectsToScan idPhotos signatures notIds initialInteractionCollider scanBTN canTransact alreadyTriggeredInitial transactionInProgress cardObj cardObj_ 
- methods: Awake(); CancelTransaction(); CheckIDHint(); CompleteTransaction(); CopyBackingFieldsToState(bool A_1); CopyStateToBackingFields(); ItemScanned(int cost); Rpc_CMD_CancelTransaction(); Rpc_CMD_CompleteTransaction(); Rpc_CMD_SetAlreadyTriggeredInitial(bool change); Rpc_CMD_StartTransaction(string npcName, Il2CppStructArray<int> items, StoreBrowseBehaviour npcScript); Rpc_CancelTransaction(); Rpc_CompleteTransaction(); Rpc_SetAlreadyTriggeredInitial(bool change); Rpc_StartTransaction(string npcName, Il2CppStructArray<int> items, StoreBrowseBehaviour npcScript); Rpc_TurnOffInteractionCollider(); SetAlreadyTriggeredInitialFalse(); SetIDValues(string name); SpawnItems(); StartTransaction(string npcName, Il2CppStructArray<int> items, StoreBrowseBehaviour npcScript); TriggerInitialInteraction(); 

## class TrapHighlight : MonoBehaviour
- fields: outline 
- methods: OnDisable(); OnEnable(); 

## class Trash : ConstrictedInteractable
- fields: trashAnim dontCountTowardHygiene targetForPet 
- methods: CopyBackingFieldsToState(bool A_1); CopyStateToBackingFields(); Delete(); DoubleCheckIfInPetsList(); Interact(PlayerManager playerMan); Rpc_Interact(PlayerRef player); Start(); 

## class TruckManager : NetworkBehaviour
- fields: deliveryItems bloodDeliveryItems droppingBloodItems bloodParticles screamSfx deliveryIndex boxSpawnPoint topGateObj truckAnim go insideTruck startVector speed gateOutline initialCheck garageDoorSwitch doorsHasBeenOpened purchaseIndex 
- methods: Awake(); CheckIfDoorOpen(); CopyBackingFieldsToState(bool A_1); CopyStateToBackingFields(); Delete(); DoHint(); DropBloodItem(); DropBox(); DropBoxes(); DropPurchase(); FixedUpdate(); Go(); RecheckHint(); Rpc_ClearPurchaseQueue(); 

## class TurnOffIfEnglish : MonoBehaviour
- fields: turnOffIfNotEnglishInstead 
- methods: OnEnable(); 

## class TurnOffIfMultiplayer : NetworkBehaviour
- fields: multiplayerObj singleplayerObj 
- methods: CheckPlayers(); CopyBackingFieldsToState(bool A_1); CopyStateToBackingFields(); Rpc_CheckObjects(int x); Spawned(); 

## class TutorialManager : NetworkBehaviour
- fields: objectiveObjs objectiveToolTips objectiveIndex tutorialObjects alreadyDone outlines noteOutline noteArrow trashBagOutline1 trashBagOutline2 trashBagOutline3 mopOutline boxOutline1 boxOutline2 disableShelfOutline limbs blood finishedBlood finishedLimbs finishedShelf checkboxSprite finishedObjectives instructions tutorialObjCanvas tutorialObjCanvasHolder tutorialObjCanvasHolderAsWell alreadyLookedAt amountToLookAtText 
- methods: Awake(); CheckForBloodDone(); CheckForLimbsDone(); CopyBackingFieldsToState(bool A_1); CopyStateToBackingFields(); FinishObjective(); FinishTutorial(); FinishedBlood(); FinishedLimbs(); FinishedShelf(); StartObjective(); 

## class TutorialShelfOutline : MonoBehaviour
- methods: Awake(); 

## class UIDService : NetworkBehaviour
- fields: fuid platformID actorID _spawned k_HostID k_msDelay k_HostActorNumber __fuidToPlatformID __fuidToActorID 
- methods: Awake(); ClearStoredIDs(); CopyBackingFieldsToState(bool A_1); CopyStateToBackingFields(); Despawned(NetworkRunner runner, bool hasState); GetFUIDForActorID(int actorID); GetFUIDForPlatformID(string platformID); OnUserJoined(string fuid, string platformID, int actorID); Spawned(); 

## class UIEventCallbacks : MonoBehaviour
- fields: onSelect onDeselect onPointerEnter onPointerDown 
- methods: OnDeselect(BaseEventData eventData); OnPointerDown(PointerEventData eventData); OnPointerEnter(PointerEventData eventData); OnSelect(BaseEventData eventData); 

## class UserInfoWidget : MonoBehaviour
- fields: parent profilePic username _updateCoroutine 
- methods: OnApplicationFocus(bool focus); Start(); StartUpdateCoroutine(); UpdateUserInfo(); WaitForPlatformThenUpdate(); 

## class ValuesToRemainAfterDeath : MonoBehaviour
- fields: npcsSpawnedBeforeDeath npcID npcKilledID carsSpawnedBefore 
- methods: Awake(); 

## class Vent : Interactable
- fields: beingChecked ventAnim checkedRecently 
- methods: Checked(); CopyBackingFieldsToState(bool A_1); CopyStateToBackingFields(); ForceOpen(); Interact(PlayerManager playerMan); InteractableAgain(); NotCheckedRecently(); Rpc_Interact(PlayerRef player); StayingInVentTooLongHint(); 

## class VentTrigger : NetworkBehaviour
- fields: ventPushOutAnim playersInVent 
- methods: CopyBackingFieldsToState(bool A_1); CopyStateToBackingFields(); EnterExitVent(bool enter, string playerName); Rpc_CMD_EnterExitVent(bool enter, string playerName); Rpc_EnterExitVent(bool enter, string playerName); 

## class VoiceVolumeController : MonoBehaviour
- fields: micGain 
- methods: OnAudioFilterRead(Il2CppStructArray<float> data, int channels); SetMicVolume(float volume); Start(); 

## class VoiceVolumeHandler : NetworkBehaviour
- fields: thisPlayerAudio _NetworkedUsername registerCoroutine 
- methods: AnnounceUsernameWhenReady(); ApplyLocalVolume(float volume); CopyBackingFieldsToState(bool A_1); CopyStateToBackingFields(); Despawned(NetworkRunner runner, bool hasState); HasLocalInputAuthority(); RefreshUsernameFromNetworkedState(string reason); RegisterWithLocalManager(); Render(); Rpc_CMD_SetPlayerName(string playerName); Spawned(); StopRegistrationCoroutine(); TryRegisterWithManager(); 

## class VoiceVolumeManager : NetworkBehaviour
- fields: scrollRectContent perPlayerVolumeRowPrefab onlySetInstanceIfInputAuthority playerVolumes playerRows voiceHandlers _queuedRefresh _refreshing discoverHandlersCoroutine 
- methods: CopyBackingFieldsToState(bool A_1); CopyStateToBackingFields(); CreateOrUpdatePlayerRow(string username, float volume); Despawned(NetworkRunner runner, bool hasState); DiscoverExistingHandlers(); DiscoverExistingHandlersAfterSpawn(); GetPlayerVolume(string username); GetVolumePlayerPrefsKey(string username); IsValidUsername(string username); RefreshLayoutAsync(); RegisterPlayer(string username, VoiceVolumeHandler voiceHandler); RemovePlayerRow(string username, VoiceVolumeHandler voiceHandler); ResetAllPlayersVolume(); SetPLayerVolume_Internal(string username, float volume); SetPlayerVolume(string username, float volume); Spawned(); UnregisterPlayer(string username, VoiceVolumeHandler voiceHandler); Update(); 

## class WFX_BulletHoleDecal : MonoBehaviour
- fields: quadUVs lifetime fadeoutpercent frames randomRotation deactivate life fadeout color orgAlpha 
- methods: Awake(); OnEnable(); holeUpdate(); 

## class WFX_Demo : MonoBehaviour
- fields: cameraSpeed orderedSpawns step range order walls bulletholes ParticleExamples exampleIndex randomSpawnsDelay randomSpawns slowMo rotateCam wood concrete metal checker woodWall concreteWall metalWall checkerWall groundTextureStr groundTextures m4 m4fps rotate_m4 
- methods: OnGUI(); OnMouseDown(); RandomSpawnsCoroutine(); SetActiveCrossVersions(GameObject obj, bool active); Update(); nextParticle(); nextTexture(); prevParticle(); prevTexture(); selectMaterial(); showHideStuff(); spawnParticle(); 

## class WFX_Demo_DeleteAfterDelay : MonoBehaviour
- fields: delay 
- methods: Update(); 

## class WFX_Demo_New : MonoBehaviour
- fields: groundRenderer groundCollider slowMoBtn slowMoLabel camRotBtn camRotLabel groundBtn groundLabel EffectLabel EffectIndexLabel AdditionalEffects ground walls bulletholes m4 m4fps wood concrete metal checker woodWall concreteWall metalWall checkerWall groundTextureStr groundTextures ParticleExamples exampleIndex slowMo defaultCamPosition defaultCamRotation onScreenParticles 
- methods: Awake(); CheckForDeletedParticles(); OnNextEffect(); OnPreviousEffect(); OnToggleCamera(); OnToggleGround(); OnToggleSlowMo(); Update(); UpdateUI(); destroyParticles(); nextParticle(); nextTexture(); prevParticle(); prevTexture(); selectMaterial(); showHideStuff(); spawnParticle(); 

## class WFX_Demo_RandomDir : MonoBehaviour
- fields: min max 
- methods: Awake(); 

## class WFX_Demo_Wall : MonoBehaviour
- fields: demo 
- methods: OnMouseDown(); 

## class WFX_LightFlicker : MonoBehaviour
- fields: time timer 
- methods: Flicker(); Start(); 

## class WalkPath : MonoBehaviour
- fields: peoplePrefabs numberOfWays lineSpacing Density _minimalObjectLength loopPath _distances pathPoint pathPointTransform points CalcPoint pointLength disableLineDraw _forward par pathType eraseRadius addPointDistance highToSpawn randXPos randZPos newPointCreation oldPointDeleting mousePosition deletePointIndex firstPointIndex secondPointIndex 
- methods: AddPoint(); Awake(); DeletePoint(); Distance(Vector3 lineStartPosition, Vector3 lineEndPosition, Vector3 pointPosition); DrawCurved(bool withDraw); GetRoutePoint(float distance, int wayIndex, int pointCount, bool forward, bool loopPath); GetRoutePosition(Il2CppStructArray<Vector3> pointArray, float distance, int pointCount, bool loopPath); PointWithLineCollision(Vector3 lineStartPosition, Vector3 lineEndPosition, Vector3 pointPosition); PointWithSphereCollision(Vector3 colisionSpherePosition, Vector3 pointPosition); SpawnOnePeople(int w, bool forward, float walkSpeed, float runSpeed); SpawnPeople(); getNextPoint(int w, int index); getPointsTotal(int w); getStartPoint(int w); 

## class WanderAroundAimlessly : NetworkBehaviour
- fields: target seeker pathfinder speed timeBeforeStartNewPathway neverStopMoving _NetworkedPosition _NetworkedRotation 
- methods: Awake(); CopyBackingFieldsToState(bool A_1); CopyStateToBackingFields(); FixedUpdate(); FixedUpdateNetwork(); Render(); Spawned(); Start(); StartNextPathway(); 

## class WeaponNodeFlashing : MonoBehaviour
- fields: sprite 
- methods: OnEnable(); 

## class WeaponSway : MonoBehaviour
- fields: xTarg yTarg rotSmooth lastKnownYRot 
- methods: Start(); Update(); 

## class WeepingAngel : Enemy
- fields: spiderSpawn anim huntMan seeker pathfinder normalSpeed curSpeed barricadeTarget chaseTarget breakingBarricade justAttackedBarricade attackBarricadeCooldown canAttack attackAudio playerMans detectionOfEachPlayer beingHit thisPlayer enemyHolder chasingPlayerTarg middleOfBody crackingBonesSfx distance detectsBeforeLookForJumpPoint playersLookingAt justLookedAway _NetworkedPosition _NetworkedRotation _IsPositionInitialised _hasSnappedToInitialPosition 
- methods: CanAttack(); CheckEnemiesLeft(float timeUntilCheck); CheckIfNearBarricade(); CompleteHunt(); CopyBackingFieldsToState(bool A_1); CopyStateToBackingFields(); DetectPlayers(); FixedUpdate(); FixedUpdateNetwork(); GetDeathAchievement(); GoToBreakBarricade(); GoToTarget(); Render(); Rpc_CMD_TogglePlayerLookAt(bool lookingAt); Rpc_ChangeHittableHealth(float health); Rpc_SetChasePlayerTargForClients(PlayerRef playerRef); Rpc_TogglePlayerLookAt(bool lookingAt); Spawned(); Start(); TogglePlayerLookAt(bool lookingAt); UpdatePlayerLists(); 

## class WorldSpaceGravity : MonoBehaviour
- fields: solver worldGravity 
- methods: Awake(); Update(); 

## class WrapRopeGameController : MonoBehaviour
- fields: solver wrappables onFinish 
- methods: Awake(); OnDisable(); OnEnable(); Solver_OnCollision(ObiSolver s, ObiNativeContactList e); Update(); 

## class WrapRopePlayerController : MonoBehaviour
- fields: acceleration rb 
- methods: Awake(); Update(); 

## class Wrappable : MonoBehaviour
- fields: wrapped normalColor wrappedColor localMaterial 
- methods: Awake(); IsWrapped(); OnDestroy(); Reset(); SetWrapped(); 

