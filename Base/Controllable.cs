using UnityEngine;

/// <summary>
/// Klasa abstrakcyjna, z której dziedziczą wszystkie komponenty,
/// którymi gracz będzie mógł sterować bezpośrednio.
/// Posiada abstrakcyjną metodę ProcessInput, którą definiujemy
/// by móc odpowiadać na akcje gracza.
/// </summary>
public abstract class Controllable : MonoBehaviour
{
	/// <summary>
	/// Przetwarza dane wejściowe w odpowiedni dla danego komponentu sposób.
	/// </summary>
	/// <param name="inputData">Dane przekazane przez PlayerController.</param>
	public abstract bool ProcessInput(InputBuffer.InputData inputData);
}
