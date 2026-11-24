using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using Utilities.ObjectPoolHandling;

namespace Game.WeaponHandling.FlameThrowingManagement
{
    public class TileFlameCatcher : MonoBehaviour, IFlameCatchable
    {
        [SerializeField] private FlameTile _flamePrefab;
        [SerializeField] private Material _fireMaterial;
        [Range(20, 50)]
        [SerializeField] private int _maxFireLimit = 25;
        [Range(0.01f, 0.3f)]
        [SerializeField] private float _beforeFlameUpdateInterval = 0.1f;
        [Range(0.1f, 0.5f)]
        [SerializeField] private float _beforeSpawnFireInterval = 0.2f;
        [Range(0.0f, 1.0f)]
        [SerializeField] private float _timeToReducePercent = 0.2f;
        [SerializeField] private TileFlameCatchData[] _flameCatchData;
        
        private Tilemap _worldTilemap;
        private Dictionary<Vector3Int, FlameTile> _flameDict;
        private ObjectPool<FlameTile> _fireObjectPool;
        private float _currentTime = 0.0f;
        private Dictionary<TileBase, TileFlameCatchData> _catchDataLookup;
        private List<Vector3Int> _flameKeysToRemove;
        private float _currentSpawnFireTime = 0.0f;
        
        private void Awake()
        {
            _worldTilemap = GetComponent<Tilemap>();
            _flameDict = new Dictionary<Vector3Int, FlameTile>();
            _catchDataLookup = new Dictionary<TileBase, TileFlameCatchData>();
            _flameKeysToRemove = new List<Vector3Int>();
        }

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            _fireObjectPool = new ObjectPool<FlameTile>(CreateTile, OnGetTile, OnReturnTile, OnDestroyTile, _maxFireLimit);
            _currentTime = 0.0f;

            foreach(var data in _flameCatchData)
            {
                _catchDataLookup.Add(data.originalTile, data);
            }
        }

        // Update is called once per frame
        void Update()
        {
            if(_currentTime >= _beforeFlameUpdateInterval)
            {
                UpdateFlames();
                RemoveTimeUpFlames();
                _currentTime = 0.0f;
                return;
            }

            _currentTime += Time.deltaTime;
        }

        public void SpawnFire(Vector3 endLinePosition, Vector3 hitPosition, Vector3 hitNormal, float maxFireRegionRadius)
        {
            /*if(_currentSpawnFireTime < _beforeSpawnFireInterval)
            {
                _currentSpawnFireTime += Time.deltaTime;
                return;
            }

            _currentSpawnFireTime = 0.0f;*/

            Vector3 internalPosition = hitPosition - hitNormal * _worldTilemap.cellSize.y / 2.0f;
            Vector3Int tilePosition = _worldTilemap.WorldToCell(internalPosition);

            BoundsInt bounds = new BoundsInt(tilePosition, new Vector3Int(Mathf.RoundToInt(maxFireRegionRadius), 1, 1));
            TileBase[] block = _worldTilemap.GetTilesBlock(bounds);
            int index = 0;
            foreach (TileBase baseTile in block)
            {
                if(baseTile == null)
                {
                    index++;
                    continue;
                }

                int x = bounds.min.x + (index % bounds.size.x);
                int y = bounds.min.y + ((index / bounds.size.x) % bounds.size.y);
                int z = bounds.min.z + (index / (bounds.size.x * bounds.size.y));

                Vector3Int positionToPlaceFlameCell = new Vector3Int(x, y, z);

                if (!_catchDataLookup.ContainsKey(baseTile))
                {
                    if(_flameDict.ContainsKey(positionToPlaceFlameCell))
                    {
                        _flameDict[positionToPlaceFlameCell].ModifyFire(_timeToReducePercent);
                    }
                    index++;
                    continue;
                }

                TileFlameCatchData data = _catchDataLookup[baseTile];

                Vector3 positionToPlaceFlame = _worldTilemap.GetCellCenterWorld(positionToPlaceFlameCell);
                                                          

                if (!_flameDict.ContainsKey(positionToPlaceFlameCell))
                {
                    FlameTile flameTile = _fireObjectPool.Get();
                    if(flameTile != null)
                    {
                        flameTile.transform.position = positionToPlaceFlame;
                        flameTile.Setup(data.burnTime, data.maxFirePercent, data.burnCurve, _fireMaterial, () => _worldTilemap.SetTile(positionToPlaceFlameCell, data.burntTile));
                        _flameDict.Add(positionToPlaceFlameCell, flameTile);
                    }
                    
                }
                index++;
            }
        }

        private void UpdateFlames()
        {
            foreach(FlameTile flame in _flameDict.Values)
            {
                flame.UpdateFlame();
            }
        }

        private void RemoveTimeUpFlames()
        {
            _flameKeysToRemove.Clear();
            foreach (var pair in _flameDict)
            {
                Vector3Int position = pair.Key;
                FlameTile flameTile = pair.Value;

                if(flameTile.CurrentElapsedTime >= flameTile.BurnTime)
                {
                    _flameKeysToRemove.Add(position);
                }
            }

            foreach(var position in _flameKeysToRemove)
            {
                _fireObjectPool.ReturnToPool(_flameDict[position]);
                _flameDict.Remove(position);
            }
        }

        private FlameTile CreateTile()
        {
            FlameTile tile = Instantiate(_flamePrefab);
            tile.gameObject.SetActive(false);
            return tile;
        }

        private void OnGetTile(FlameTile tile)
        {
            tile.gameObject.SetActive(true);
        }

        private void OnReturnTile(FlameTile tile)
        {
            tile.gameObject.SetActive(false);
        }

        private void OnDestroyTile(FlameTile tile)
        {
            Destroy(tile.gameObject);
        }
    }
}
