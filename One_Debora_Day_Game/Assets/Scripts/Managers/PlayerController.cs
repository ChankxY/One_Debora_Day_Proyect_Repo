using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed;
    public float jumpForce;
    public Rigidbody rig;

    private bool isGrounded;
    public int score;
    public TextMeshProUGUI scoreText;

    void Update()
    {
//Tomar los inpouts del teclado 
float x = Input.GetAxisRaw("Horizontal")*moveSpeed;
float z = Input.GetAxisRaw("Vertical")*moveSpeed;

//Iniciar velocidad 
rig.linearVelocity = new Vector3(x , rig.linearVelocity.y, z);

//crear velocidad temporal y cancelar el Y
Vector3 vel = rig.linearVelocity;
vel.y = 0;

//Organiozar el movimiento y la rotación en la dirección 
if(vel.x != 0 || vel.z !=0)
        {
            transform.forward = vel;
        }

        //La logica para el salto 
        if(Input.GetKeyDown(KeyCode.Space) && isGrounded == true)
        {
            isGrounded = false;
            rig.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }
        //Poner un limite para el game over
        if(transform.position.y < -5)
        {
            GameOver();
        }
    }
    private void OllisionEnter(Collision collision)
    {
 //Validación de colisión con la plataforma 
 if(collision.GetContact(0).normal == Vector3.up)
        {
            isGrounded = true;
        }       
    }
    public void GameOver()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
    public void AddScore(int amount)
    {
        score += amount;
        scoreText.text = score.ToString();
    }
}
