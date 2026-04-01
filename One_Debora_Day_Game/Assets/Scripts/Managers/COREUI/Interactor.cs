using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
public class Interactor : MonoBehaviour
{
    public int numeroEscena;
    public GameObject Texto;
    private bool Lugar;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.E) && Lugar == true)
        {
            SceneManager.LoadScene(numeroEscena);

        }
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player")
        {
            Texto.SetActive(true);
            Lugar = true;
        }
    }
    private void OnTriggerExit(Collider other)
    {
        {
            if (other.tag == "Player")
            {
                Texto.SetActive(false);
                Lugar = false;
            }
        }
    }
}
