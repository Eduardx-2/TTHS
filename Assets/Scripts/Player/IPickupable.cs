// Cualquier objeto que se pueda recoger del piso implementa esta interfaz.
// El PlayerController detecta objetos cercanos que la implementen y llama a OnPickedUp
// cuando el jugador presiona la tecla de recoger.
public interface IPickupable
{
    void OnPickedUp(PlayerController player);
}
