using UnityEngine;

public class AudioMenager : MonoBehaviour
{
    public AudioSource explosionSound;
    public AudioSource shootSound;
    public AudioSource moveSound;
    public AudioSource jumpSound;
    public AudioSource damageSound;

    public void PlaySoundExplosion()
    {
        explosionSound.Play();
    }
    public void PlaySoundShoot()
    {
        shootSound.Play();
    }
    public void PlaySoundMove()
    {
        moveSound.Play();
    }
    public void StopSoundMove()
    {
        moveSound.Stop();
    }
    public void PlaySoundJump()
    {
        jumpSound.Play();
    }
    public void PlaySoundDamage()
    {
        damageSound.Play();
    }
    public bool IsSoundMovePlaying() { return moveSound.isPlaying; }
}
