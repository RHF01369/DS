//using System.Collections;
//using System.Collections.Generic;
//using System.Linq;
//using UnityEngine;
//using UnityEngine.UI;

//public class BattleHandler : MonoBehaviour {

//    [SerializeField] protected PatternLock          patternLock;
//    [SerializeField] protected PositionResercher    playerPositionResercher;
//    [SerializeField] protected PositionResercher    enemyPositionResercher;

//    [SerializeField] protected SummonSpawn          playerSpawn;
//    [SerializeField] protected SummonSpawn          enemySpawn;

//    [SerializeField] protected SummonInfoManager    summonInfoManager;
//    [SerializeField] protected DeckDataManager      deckDataManager;

//    [SerializeField] protected Text                 playerHealthState;
//    [SerializeField] protected Text                 enemyHealthState;

//    protected Dictionary<string, UnitData> deckDataOfPattern;
//    protected List<UnitData> playerDeck;
//    protected List<UnitData> enemyDeck;

//    protected UserData userData;

//    protected void InitPlayerDeck()
//    {
//        playerDeck.Clear();

//        deckDataManager.SetMyDeck();
//    }

//    protected void InitDeckDataOfPattern()
//    {
//        deckDataOfPattern.Clear();

//        for (int i = 0; i < playerDeck.Count; i++)
//            deckDataOfPattern.Add(playerDeck[i].pattern, playerDeck[i]);
//    }

//    protected bool WhetherTheDeckHasChanged()
//    {
//        if (deckDataOfPattern.Count == 0)
//            return true;

//        int index = 0;
//        return deckDataOfPattern.Values.Any(deckData => deckData.number != userData.SummonNumbersOfDeck[index++]);
//    }

//    public virtual void BattleStart() { }
//    public virtual void InputPattern(string pattern) { }
//}