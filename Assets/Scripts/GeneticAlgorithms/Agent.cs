using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace IA.GeneticAlgorithm
{
    public struct Instruction
    {
        public Vector2 movementVector;
        public float force;
    }

    public class Agent : MonoBehaviour
    {
        /*
        Las prioridades del agente son:
        - LLegar lo mas cerca posible de la plataforma
        - LLegar a la plataforma en el menor tiempo posible
        */

        [HideInInspector]
        public Chromosome chromosome;
        [HideInInspector]
        public Method method;
        private Gene currentGene;
        private int i = 0;
        private float timer = 0;
        private Rigidbody2D rb;
        private bool simulationFinished = false;
        private bool touchedFloor = false;

        public NeuronalNetworks.Network network;
        private Instruction instructions;
        
        private float distanceToTarget;
        private float timeToTarget;
        private float score;
        private Transform target;

        public void Init()
        {
            rb = GetComponent<Rigidbody2D>();
            target = GameObject.FindWithTag("Target").transform;

            if (method == Method.Genetic)
            {
                currentGene = chromosome.genes[i];
                timer = currentGene.time;
            }
            else if (method == Method.NeuronalNetwork)
            {
                transform.Translate(Vector3.right * 8.0f * Random.Range(-1.0f, 1.0f));

                Vector2 pos = new Vector2(transform.position.x, transform.position.y);
                Vector2 tgt = new Vector2(target.position.x, target.position.y);
                Vector2 dir = tgt - pos;
                Vector2 vel = rb.velocity.normalized;
                dir.Normalize();

                float[] input = new float[4];
                input[0] = dir.x;
                input[1] = dir.y;
                input[2] = vel.x;
                input[3] = vel.y;
                float[] output = network.Process(input);

                Instruction i = new Instruction();
                i.movementVector.x = output[0] * 2 - 1;
                i.movementVector.y = output[1] * 2 - 1;
                i.force = output[2] * 25;
                instructions = i;
            }
        }

        private void FixedUpdate()
        {
            if (!simulationFinished)
            {
                if (timer > 0)
                {
                    if (!touchedFloor)
                    {
						if (method == Method.Genetic)  //Aca el mono hace su gracia xD (aka se realizan las acciones de acuerdo a la informacion en los genes del cromosoma del agente)
                        {
                            rb.AddForce(Vector2.right * currentGene.horizontalAxisAmmount * Time.fixedDeltaTime, ForceMode2D.Impulse);
                            rb.AddForce(Vector2.up * currentGene.verticalAxisAmmount * Time.fixedDeltaTime, ForceMode2D.Impulse);
                        }
                    }
                    timer -= Time.fixedDeltaTime;
                }
                else
                {
                    if (method == Method.Genetic)
                        GetNextGene();
                }

                if (method == Method.NeuronalNetwork && !touchedFloor)
                {
                    Vector2 pos = new Vector2(transform.position.x, transform.position.y);
                    Vector2 tgt = new Vector2(target.position.x, target.position.y);
                    Vector2 dir = tgt - pos;
                    Vector2 vel = instructions.movementVector;
                    dir.Normalize();

                    float[] input = new float[4];
                    input[0] = dir.x;
                    input[1] = dir.y;
                    input[2] = vel.x;
                    input[3] = vel.y;
                    float[] output = network.Process(input);
                    
                    instructions.movementVector.x = output[0] * 2 - 1;
                    instructions.movementVector.y = output[1];
                    instructions.force = output[2] * 25;

                    rb.AddForce(instructions.movementVector * instructions.force * Time.fixedDeltaTime, ForceMode2D.Impulse); 
                }

                if (!touchedFloor)
                    timeToTarget += Time.fixedDeltaTime;
            }
        }

        private void OnCollisionEnter2D(Collision2D c)  //Verifico si el agente toco el suelo
        {
            touchedFloor = true;
            rb.velocity = Vector3.zero;
        }

		private void GetNextGene()  //Obtengo el siguiente gen del cromosoma (NO OLVIDAR! El cromosoma "es el agente" porque tiene toda su info)
        {
            i++;
            if (i != chromosome.geneCount)
            {
                currentGene = chromosome.genes[i];
                timer = currentGene.time;
            }
            else
            {
				simulationFinished = true;  //se acabaron los genes; termino la simulacion
            }
        }

        public void CalculateScore() //Calculo el puntaje obtenido por el agente de acuerdo a las prioridades arriba mencionadas
        {
            score = 0;
            distanceToTarget = Vector3.Distance(transform.position, target.position);
            score += MathS.InverseProportion(distanceToTarget, 10.0f);
            if(distanceToTarget <= 1.0f && touchedFloor)
                if(method == Method.Genetic)
                    score += MathS.InverseProportion(timeToTarget, 10.0f);
        }

        public float GetScore()  //Getter porque el puntaje es privado
        {
            return score;
        }
    }
}