using SpawnPointSelector;

namespace NomaiGrandPrix
{
    /// <summary>
    /// An extension of Menu. In addition to handling some special logic for enabling/disabling
    /// the menu, this is also used by TitleScreenManagerPatches to identify when certain
    /// aspects of the title screen should be enabled/disabled.
    /// </summary>
    public class SpawnPointMenu : Menu
    {
        public override void EnableMenu(bool shouldEnable)
        {
            _menuActivationRoot = gameObject;
            base.EnableMenu(shouldEnable);
            _menuActivationRoot.gameObject.SetActive(true);
            gameObject.GetComponent<SpawnPointList>().SetCollapsed(!shouldEnable);
        }
    }
}
