namespace DeadAir.Core
{
    // Всё, во что можно попасть и нанести урон
    public interface IDamageable
    {
        bool IsDead { get; }
        void TakeDamage(float damage);
    }
}
