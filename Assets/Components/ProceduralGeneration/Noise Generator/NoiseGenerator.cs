using System;
using System.Collections.Generic;
using System.Threading;
using Components.ProceduralGeneration;
using Cysharp.Threading.Tasks;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Serialization;
using VTools.Grid;

namespace Components.ProceduralGeneration.NoiseGenerator
{
    [CreateAssetMenu(menuName = "Procedural Generation Method/Noise Generator")]
    public class NoiseGenerator : ProceduralGenerationMethod
    {
        [Header("Noise Parameters")]
        [SerializeField] private FastNoiseLite.NoiseType noiseType = FastNoiseLite.NoiseType.OpenSimplex2;
        [SerializeField, Range(0, 1)] private float frequency = 0.0296f;
        [SerializeField, Range(0, 2)] private float amplitude = 1.1f;
        
        [Header("Fractal Parameters")]
        [SerializeField] private FastNoiseLite.FractalType fractalType = FastNoiseLite.FractalType.FBm;
        [SerializeField, Range(0, 10)] private int octaves = 2;
        [SerializeField, Range(0, 10)] private float lacunarity = 1.2f;
        [SerializeField, Range(0, 10)] private float gain = 0.5f;
        
        [Header("Heights")]
        [SerializeField, Range(-1, 1)] private float waterHeight = -0.92f;
        [SerializeField, Range(-1, 1)] private float sandHeight = -0.41f;
        [SerializeField, Range(-1, 1)] private float grassHeight = 0.58f;
        [SerializeField, Range(-1, 1)] private float rockHeight = 1f;
        
        [Header("Debug")]
        [SerializeField] private bool drawDebugTexture = false;
        [SerializeField] private float debugTextureAlpha = 1f;
        
        [NonSerialized] private float[,] _noiseMap;
        
        protected override async UniTask ApplyGeneration(CancellationToken cancellationToken)
        {
            FastNoiseLite noise = SetupNoise();
            
            GenerateNoiseMap(noise);

            await ApplyNoiseMapToGrid(cancellationToken);
            
            if (drawDebugTexture)
            {
                DrawDebugTexture();
            }
        }
        
        private FastNoiseLite SetupNoise()
        {
            FastNoiseLite noise = new FastNoiseLite(RandomService.Seed);
            
            noise.SetNoiseType(noiseType);
            noise.SetFrequency(frequency);
            noise.SetFractalType(fractalType);
            noise.SetFractalOctaves(octaves);
            noise.SetFractalLacunarity(lacunarity);
            noise.SetFractalGain(gain);
            
            return noise;
        }

        private void GenerateNoiseMap(FastNoiseLite noise)
        {
            _noiseMap = new float[Grid.Width, Grid.Lenght];
            
            for (int x = 0; x < Grid.Width; x++)
            {
                for (int y = 0; y < Grid.Lenght; y++)
                {
                    float noiseValue = noise.GetNoise(x, y) * amplitude;
                    
                    noiseValue = Mathf.Clamp(noiseValue, -1f, 1f);
                    
                    _noiseMap[x, y] = noiseValue;
                }
            }
        }
        
        private async UniTask ApplyNoiseMapToGrid(CancellationToken cancellationToken)
        {
            if (_maxSteps > 1)
            {
                int rowsPerStep = Mathf.Max(1, Grid.Lenght / _maxSteps);
                
                for (int step = 0; step < _maxSteps; step++)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    
                    int startY = step * rowsPerStep;
                    int endY = Mathf.Min(startY + rowsPerStep, Grid.Lenght);
                    
                    for (int x = 0; x < Grid.Width; x++)
                    {
                        for (int y = startY; y < endY; y++)
                        {
                            if (Grid.TryGetCellByCoordinates(x, y, out Cell cell))
                            {
                                string tileName = GetTileNameFromNoiseValue(_noiseMap[x, y]);
                                AddTileToCell(cell, tileName, true);
                            }
                        }
                    }
                    
                    await UniTask.Delay(GridGenerator.StepDelay, cancellationToken: cancellationToken);
                }
            }
            else
            {
                for (int x = 0; x < Grid.Width; x++)
                {
                    for (int y = 0; y < Grid.Lenght; y++)
                    {
                        cancellationToken.ThrowIfCancellationRequested();
                        
                        if (Grid.TryGetCellByCoordinates(x, y, out Cell cell))
                        {
                            string tileName = GetTileNameFromNoiseValue(_noiseMap[x, y]);
                            AddTileToCell(cell, tileName, true);
                        }
                    }
                }
                
                await UniTask.Yield();
            }
        }
        
        private string GetTileNameFromNoiseValue(float noiseValue)
        {
            if (noiseValue <= waterHeight)
                return WATER_TILE_NAME;
            else if (noiseValue <= sandHeight)
                return SAND_TILE_NAME;
            else if (noiseValue <= grassHeight)
                return GRASS_TILE_NAME;
            else
                return ROCK_TILE_NAME;
        }
        
        private void DrawDebugTexture()
        {
            if (_noiseMap == null) return;
            
            Texture2D debugTexture = new Texture2D(Grid.Width, Grid.Lenght);
            
            for (int x = 0; x < Grid.Width; x++)
            {
                for (int y = 0; y < Grid.Lenght; y++)
                {
                    float normalizedValue = (_noiseMap[x, y] + 1f) * 0.5f;
                    Color color = new Color(normalizedValue, normalizedValue, normalizedValue, debugTextureAlpha);
                    debugTexture.SetPixel(x, y, color);
                }
            }
            
            debugTexture.Apply();
            
            Debug.Log($"Debug texture created: {Grid.Width}x{Grid.Lenght}");
            
            #if UNITY_EDITOR
            byte[] bytes = debugTexture.EncodeToPNG();
            string path = UnityEngine.Application.dataPath + "/NoiseDebug_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".png";
            System.IO.File.WriteAllBytes(path, bytes);
            Debug.Log($"Debug texture saved to: {path}");
            UnityEditor.AssetDatabase.Refresh();
            #endif
            
            UnityEngine.Object.Destroy(debugTexture);
        }

        public float GetNoiseValueAt(int x, int y)
        {
            if (_noiseMap == null || x < 0 || x >= Grid.Width || y < 0 || y >= Grid.Lenght)
                return 0f;
            
            return _noiseMap[x, y];
        }
    }
}