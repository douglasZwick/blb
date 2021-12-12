/***************************************************
File:           GlobalData.cs
Authors:        Christopher Onorati
Last Updated:   6/19/2019
Last Version:   2019.1.4

Description:
  Houses global data useful across many scripts and
  systems.

Copyright 2018-2019, DigiPen Institute of Technology
***************************************************/

public enum TileType
{
  EMPTY,
  SOLID,
  SLOPE_LEFT,
  SLOPE_RIGHT,
  SLOPE_LEFT_INV,
  SLOPE_RIGHT_INV,
  START,
  DEADLY,
  GOAL,
  FALSE_SOLID,
  INVISIBLE_SOLID,
  CHECKPOINT,
  TELEPORTER,
  DOOR,
  KEY,
  COIN,
  SWITCH,
  BOOSTER,
  BG,
  BG_LEFT,
  BG_RIGHT,
  BG_LEFT_INV,
  BG_RIGHT_INV,
  MOVESTER,
  GOON,
}

public enum TileColor
{
  RED,
  ORANGE,
  YELLOW,
  GREEN,
  CYAN,
  BLUE,
  PURPLE,
  MAGENTA,
}

public enum Direction
{
  RIGHT,
  LEFT,
  UP,
  DOWN,
}

/**
* CLASS NAME  : GlobalData
* DESCRIPTION : Simple variable-storing class.
**/
public static class GlobalData
{
  public static string s_Version = "0.9.40";

  /************************************************************************************/
  /************************************************************************************/
  /**********************************PLAY MODE STATUS**********************************/
  /************************************************************************************/
  /************************************************************************************/

  //Get the play mode status.
  static bool s_IsInPlayMode = false;
  static bool s_Transitioning = false;
  static bool s_EffectsUnderway = false;

  //Event to register playmode switching.
  public delegate void ParameterlessEvent();
  public delegate void BooleanEvent(bool booleanParameter);
  public delegate void IntEvent(int intParameter);
  public delegate void ColorCodeEvent(TileColor tileColor);
  public delegate void PlayModeEvent(PlayModeEventData eventData);

  public static event ParameterlessEvent HeroDied;
  public static event ParameterlessEvent PreHeroReturn;
  public static event ParameterlessEvent HeroReturned;
  public static event ParameterlessEvent GhostModeEnabled;
  public static event ParameterlessEvent GhostModeDisabled;
  public static event ParameterlessEvent GhostCleanup;
  public static event ParameterlessEvent ColorblindModeEnabled;
  public static event ParameterlessEvent ColorblindModeDisabled;
  public static event ParameterlessEvent OnFpsSwitched;
  public static event ParameterlessEvent EditingEnabled;
  public static event ParameterlessEvent EditingDisabled;

  public static event PlayModeEvent PreToggleEffects;
  public static event PlayModeEvent PostToggleEffects;

  public static event BooleanEvent PlayModePreToggle;
  public static event BooleanEvent PlayModeToggled;
  public static event BooleanEvent ModeStarted;

  public static event IntEvent CoinCollected;

  public static event ColorCodeEvent KeyCollected;
  public static event ColorCodeEvent SwitchActivated;

  /**
  * FUNCTION NAME: IsInPlayMode
  * DESCRIPTION  : Getter to check the play mode status.
  * INPUTS       : None
  * OUTPUTS      : None
  **/
  public static bool IsInPlayMode()
  {
    return s_IsInPlayMode;
  }


  public static bool IsTransitioning()
  {
    return s_Transitioning;
  }


  public static bool AreEffectsUnderway()
  {
    return s_EffectsUnderway;
  }


  public static void TogglePlayMode()
  {
    if (s_EffectsUnderway)
      return;

    var eventData = new PlayModeEventData()
    {
      m_IsInPlayMode = s_IsInPlayMode,
    };

    PreToggleEffects?.Invoke(eventData);

    if (eventData.m_Handled)
      s_EffectsUnderway = true;
    else
      ToggleHelper();
  }


  public static void PreToggleEffectsEnded()
  {
    s_EffectsUnderway = false;

    ToggleHelper();

    var eventData = new PlayModeEventData()
    {
      m_IsInPlayMode = s_IsInPlayMode,
    };

    PostToggleEffects?.Invoke(eventData);

    if (eventData.m_Handled)
      s_EffectsUnderway = true;
    else
      ModeStartedHelper();
  }


  public static void PostToggleEffectsEnded()
  {
    ModeStartedHelper();
  }


  static void ToggleHelper()
  {
    if (s_IsInPlayMode)
      DisablePlayMode();
    else
      EnablePlayMode();
  }


  static void ModeStartedHelper()
  {
    s_EffectsUnderway = false;

    ModeStarted?.Invoke(s_IsInPlayMode);
  }


  /**
  * FUNCTION NAME: EnablePlayMode
  * DESCRIPTION  : Set the game as in play mode.
  * INPUTS       : None
  * OUTPUTS      : bool - Informs the activator that the playmode was set.
  **/
  public static bool EnablePlayMode()
  {
    s_Transitioning = true;

    //Call the event if anything is attached.
    PlayModePreToggle?.Invoke(true);
    PlayModeToggled?.Invoke(true);

    s_IsInPlayMode = true;

    s_Transitioning = false;

    return true;
  }

  /**
  * FUNCTION NAME: DisablePlayMode
  * DESCRIPTION  : Set the game as no longer in play mode.
  * INPUTS       : None
  * OUTPUTS      : bool - Informs the activator that the playmode was deactivated.
  **/
  public static bool DisablePlayMode()
  {
    s_Transitioning = true;

    //Call the event if anything is attached.
    PlayModePreToggle?.Invoke(false);
    PlayModeToggled?.Invoke(false);

    s_IsInPlayMode = false;

    s_Transitioning = false;

    return false;
  }

  public static void DispatchHeroDied()
  {
    HeroDied?.Invoke();
  }

  public static void DispatchPreHeroReturn()
  {
    PreHeroReturn?.Invoke();
  }

  public static void DispatchHeroReturned()
  {
    HeroReturned?.Invoke();
  }

  public static void DispatchCoinCollected(int value)
  {
    CoinCollected?.Invoke(value);
  }

  public static void DispatchGhostModeEnabled()
  {
    GhostModeEnabled?.Invoke();
  }

  public static void DispatchGhostModeDisabled()
  {
    GhostModeDisabled?.Invoke();
  }

  public static void DispatchGhostCleanup()
  {
    GhostCleanup?.Invoke();
  }

  public static void DispatchColorblindModeEnabled()
  {
    ColorblindModeEnabled?.Invoke();
  }

  public static void DispatchColorblindModeDisabled()
  {
    ColorblindModeDisabled?.Invoke();
  }

  public static void DispatchFpsCapSwitched()
  {
    OnFpsSwitched?.Invoke();
  }

  public static void DispatchEditingEnabled()
  {
    EditingEnabled?.Invoke();
  }

  public static void DispatchEditingDisabled()
  {
    EditingDisabled?.Invoke();
  }

  public static void DispatchKeyCollected(TileColor keyColor)
  {
    KeyCollected?.Invoke(keyColor);
  }

  public static void DispatchSwitchActivated(TileColor switchColor)
  {
    SwitchActivated?.Invoke(switchColor);
  }

  /************************************************************************************/
  /************************************************************************************/
  /************************************CURRENT TILE************************************/
  /************************************************************************************/
  /************************************************************************************/

  //Currently selected primary (left mouse) tile.
  static TileType m_SelectedPrimaryTile = TileType.SOLID;

  //Currently selected secondary (right mouse) tile.
  static TileType m_SelectedSecondaryTile = TileType.EMPTY;

  //Events to register tile selection changing.
  public delegate void TileTypeEvent(TileType _tileType);
  public static event TileTypeEvent PrimaryTileChanged;
  public static event TileTypeEvent SecondaryTileChanged;

  /**
  * FUNCTION NAME: GetSelectedPrimaryTile
  * DESCRIPTION  : Get the primary selected tile.
  * INPUTS       : None
  * OUTPUTS      : TileType - ID of the primary tile.
  **/
  public static TileType GetSelectedPrimaryTile()
  {
    return m_SelectedPrimaryTile;
  }

  /**
  * FUNCTION NAME: SetPrimarySelectedTile
  * DESCRIPTION  : Set the primary selected tile.
  * INPUTS       : _tileType - Tile to use for primary input.
  * OUTPUTS      : None
  **/
  public static void SetPrimarySelectedTile(TileType _tileType)
  {
    //Not a real update.
    if (_tileType == m_SelectedPrimaryTile)
      return;

    m_SelectedPrimaryTile = _tileType;

    //Call the event if anything is attached.
    PrimaryTileChanged?.Invoke(m_SelectedPrimaryTile);
  }

  /**
  * FUNCTION NAME: GetSelectedSecondaryTile
  * DESCRIPTION  : Get the secondary selected tile.
  * INPUTS       : None
  * OUTPUTS      : TileType - ID of the secondary tile.
  **/
  public static TileType GetSelectedSecondaryTile()
  {
    return m_SelectedSecondaryTile;
  }

  /**
  * FUNCTION NAME: SetSecondarySelectedTile
  * DESCRIPTION  : Set the secondary selected tile.
  * INPUTS       : _tileType - Tile to use for secondary input.
  * OUTPUTS      : None
  **/
  public static void SetSecondarySelectedTile(TileType _tileType)
  {
    //Not a real update.
    if (_tileType == m_SelectedSecondaryTile)
      return;

    m_SelectedSecondaryTile = _tileType;

    //Call the event if anything is attached.
    SecondaryTileChanged?.Invoke(m_SelectedSecondaryTile);
  }
}


public class PlayModeEventData
{
  public bool m_IsInPlayMode;
  public bool m_Handled = false;
}
