using Game.Entities;
using Models.Static;
using Networking;
using Networking.Packets.Outgoing;
using UnityEngine;

namespace Game
{
    public class PlayerShootController
    {
        private readonly Player _player;

        private int _attackPeriod;
        private int _attackStart;
        private int _time;
        
        private int _nextProjectileId;

        public PlayerShootController(Player player)
        {
            _player = player;
        }

        public void Tick(int time, Camera camera)
        {
            _time = time;
            
            if (Input.GetMouseButton(0))
            {
                var mousePosition = Input.mousePosition;
                var viewportPoint = camera.ScreenToViewportPoint(mousePosition);
                if (viewportPoint.x < 0 || viewportPoint.x > 1 || viewportPoint.y < 0 || viewportPoint.y > 1)
                    return;
                
                var playerPosition = camera.WorldToScreenPoint(_player.Position);
                var angle = Mathf.Atan2(mousePosition.y - playerPosition.y, mousePosition.x - playerPosition.x);
                TryShoot(angle + _player.Rotation);
            }
        }

        private void TryShoot(float attackAngle)
        {
            if (_player.HasConditionEffect(ConditionEffect.Stunned))
                return;

            var weaponType = _player.Equipment[0];
            var itemData = _player.ItemDatas[0];
            if (weaponType == -1)
                return;

            var weaponXml = AssetLibrary.GetItemDesc(weaponType);
            var rateOfFireMod = ItemDesc.GetStat(itemData, ItemData.RateOfFire, ItemDesc.RATE_OF_FIRE_MULTIPLIER);
            var rateOfFire = weaponXml.RateOfFire;
            
            rateOfFire *= 1 + rateOfFireMod;
            _attackPeriod = (int)(1 / _player.GetAttackFrequency() * (1 / rateOfFire));
            _player.AttackPeriod = _attackPeriod;
            if (_time < _attackStart + _attackPeriod)
                return;
            
            _attackStart = _time;
            _player.AttackStart = _time;
            _player.AttackAngle = attackAngle;
            Shoot(_attackStart, weaponType, itemData, weaponXml, attackAngle, false);
        }

        private void Shoot(int time, int weaponType, int itemData, ItemDesc weaponXml, float attackAngle,
            bool isAbility)
        {
            var numShots = weaponXml.NumProjectiles;
            var arcGap = weaponXml.ArcGap * Mathf.Deg2Rad;
            var totalArc = arcGap * (numShots - 1);
            var angle = attackAngle - totalArc / 2;
            var damageMod = ItemDesc.GetStat(itemData, ItemData.Damage, ItemDesc.DAMAGE_MULTIPLIER);
            var startId = _nextProjectileId;
            _nextProjectileId -= numShots;
            
            for (var i = 0; i < numShots; i++)
            {
                var minDamage = weaponXml.Projectile.MinDamage + (int)(weaponXml.Projectile.MinDamage * damageMod);
                var maxDamage = weaponXml.Projectile.MaxDamage + (int)(weaponXml.Projectile.MaxDamage * damageMod);
                var damage = (int)(_player.Random.NextIntRange((uint) minDamage, (uint) maxDamage) *
                             _player.GetAttackMultiplier());
                var projectile = Projectile.Create(_player, weaponXml.Projectile, startId - i, time, angle,
                    _player.Position, damage, _player.Map);

                _player.Map.AddObject(projectile, projectile.StartPosition);
                angle += arcGap;
            }
            
            TcpTicker.Send(new PlayerShoot(time, _player.Position, attackAngle, isAbility, numShots));
        }
    }
}