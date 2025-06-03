// ============================================================================
// 1. GameManager.cs - 게임 전체 관리 (싱글톤)
// ============================================================================
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Game Settings")]
    public int maxBots = 8;
    public float botSpawnInterval = 3f;
    public Transform[] spawnPoints;
    public GameObject[] botPrefabs;

    [Header("UI")]
    public GameObject gameOverUI;
    public TMPro.TextMeshProUGUI aliveCountText;

    private List<BaseCharacter> aliveCharacters = new();
    private float lastBotSpawnTime;

    void Awake() => Instance = this;

    void Start()
    {
        // 플레이어는 Scene에 이미 배치되어 있다고 가정
        var player = FindAnyObjectByType<PlayerController>();
        if (player) RegisterCharacter(player);

        SpawnInitialBots();
    }

    void Update()
    {
        UpdateUI();
        TrySpawnBots();
        CheckGameEnd();
    }

    void SpawnInitialBots()
    {
        for (int i = 0; i < maxBots; i++)
        {
            SpawnRandomBot();
        }
    }

    void TrySpawnBots()
    {
        if (Time.time - lastBotSpawnTime > botSpawnInterval &&
            aliveCharacters.Count(c => c is BotController) < maxBots)
        {
            SpawnRandomBot();
            lastBotSpawnTime = Time.time;
        }
    }

    void SpawnRandomBot()
    {
        var spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];
        var botPrefab = botPrefabs[Random.Range(0, botPrefabs.Length)];
        var bot = Instantiate(botPrefab, spawnPoint.position, Quaternion.identity);
        RegisterCharacter(bot.GetComponent<BaseCharacter>());
    }

    public void RegisterCharacter(BaseCharacter character)
    {
        aliveCharacters.Add(character);
        character.OnDeath += () => aliveCharacters.Remove(character);
    }

    void UpdateUI()
    {
        if (aliveCountText)
            aliveCountText.text = $"Alive: {aliveCharacters.Count}";
    }

    void CheckGameEnd()
    {
        var alivePlayers = aliveCharacters.OfType<PlayerController>().Count();
        if (alivePlayers == 0)
        {
            GameOver(false); // 플레이어 패배
        }
        else if (aliveCharacters.Count == 1 && alivePlayers == 1)
        {
            GameOver(true); // 플레이어 승리
        }
    }

    void GameOver(bool playerWon)
    {
        Time.timeScale = 0;
        if (gameOverUI) gameOverUI.SetActive(true);
        // 승리/패배 UI 표시
    }
}











