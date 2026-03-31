namespace Entities
{
    public class DestroyableWall : Entity
    {
        public override int TakeDamage(int _, Entity __)
        {
            _currentHealth--;

            if (_currentHealth <= 0)
            {
                OnDeath?.Invoke(this);
                Destroy(gameObject);
            }
            
            return 1;
        }
    }
}