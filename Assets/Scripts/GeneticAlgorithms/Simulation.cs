using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace IA.GeneticAlgorithm
{
    public enum Method
    {
        Genetic,
        NeuronalNetwork
    };

    public enum Mode
    {
        Auto,
        Manual
    };

    public class Simulation : MonoBehaviour
    {
		[HideInInspector]
		public Mode mode = Mode.Auto;
        public Method method;
        public GameObject agent;
        public Transform target;
        public Transform startPosition;
        public float maxSimulationTime;
        public int agentsPerGeneration;
        public int genesPerChromosome;
		[HideInInspector]
        [Range(0, 1)]
        public float eliteProportion;
		[HideInInspector]
        [Range(0, 1)]
        public float mutationChance;
		[HideInInspector]
        [Range(0, 1)]
        public float mutationThreshold;
        public Text simText;
        public int inputLayerSize;
        public int hiddenLayerSize;
        public int hiddenLayerQuant;
        public int outputLayerSize;
        public float bias;

        private List<Agent> agents = new List<Agent>();
        private bool simulationInProgress = false;
        private float simulationTimer;
        private float lastAvg;
        private int currentGeneration;

        private void Start()
        {
            if (mode == Mode.Auto)
                Time.timeScale = 4;

            if (agentsPerGeneration % 2 != 0)
                agentsPerGeneration++;
        }

        private void Update()
        {
            if (simulationInProgress)
            {
                simText.text = "Simulating (Time left: " + simulationTimer.ToString("F2") + "). Press \"Enter\" to stop simulation. Last generation average score: " + lastAvg.ToString("F2") + ".\nCurrent generation: " + currentGeneration;
            }
            else
            {
                simText.text = "Press \"Enter\" to start simulation. Last generation average score: " + lastAvg.ToString("F2") + ".\nCurrent generation: " + currentGeneration;
            }

            if (Input.GetKeyDown(KeyCode.Return) && !simulationInProgress)
            {
                StartSimulation();
            }
            else if (Input.GetKeyDown(KeyCode.Return))
            {
                Cleanup();
            }

            if (mode == Mode.Manual)
            {
                if (Input.GetKey(KeyCode.Keypad0))
                {
                    Time.timeScale = 4;
                }
                else
                {
                    Time.timeScale = 1;
                }
            }

            if (simulationTimer > 0)
            {
                simulationTimer -= Time.deltaTime;

                if(simulationTimer <= 0)
                {
                    Cleanup();
                    if (mode == Mode.Auto)
                        StartSimulation();
                }
            }
        }

        private void StartSimulation()
        {
            if (currentGeneration == 0)
                GenerateFirstGeneration();  //Mando a crear la primera generacion
            else
            {
                currentGeneration++;
                simulationTimer = maxSimulationTime;
                simulationInProgress = true;
                foreach(Agent a in agents)
                {
                    a.gameObject.SetActive(true);
                }
            }
        }

        private void GenerateFirstGeneration()
        {
            currentGeneration++;
            simulationTimer = maxSimulationTime;
            simulationInProgress = true;

            for (int i = 0; i < agentsPerGeneration; i++)
            {
                GameObject objRef = Instantiate(agent, startPosition.position, Quaternion.identity);
                Agent objAgent = objRef.GetComponent<Agent>();

                if (method == Method.Genetic)
                {
					objAgent.method = Method.Genetic;  //Asigno el metodo a desarrollar por la simulacion
                    objAgent.chromosome = new Chromosome();
					objAgent.chromosome.geneCount = genesPerChromosome;  // Inicizlizo los cromosomas y los genes
                    objAgent.chromosome.genes = new Gene[genesPerChromosome];
                    for (int j = 0; j < genesPerChromosome; j++)
                    {
                        objAgent.chromosome.genes[j].horizontalAxisAmmount = Random.Range(-10.0f, 10.0f);  // Le asigno valores a los componentes de los genes
                        objAgent.chromosome.genes[j].verticalAxisAmmount = Random.Range(0.0f, 10.0f);
                        objAgent.chromosome.genes[j].time = Random.Range(0.125f, 0.75f);
                    }
                }
                else if (method == Method.NeuronalNetwork)
                {
                    objAgent.method = Method.NeuronalNetwork;  //Asigno el metodo a desarrollar por la simulacion
                    objAgent.network = new NeuronalNetworks.Network();  //Creo la nueva red neuronal
                    objAgent.network.Init(inputLayerSize, hiddenLayerSize, hiddenLayerQuant, outputLayerSize, bias);  //Inicializo las capas de la nueva red neuronal
                    objAgent.network.RandomizeWeights(-5.0f, 5.0f, 0.0f, 2.5f);  //Asigno valores aleatorios a los distintos pesos que pose la red en las distintas capas
                }

                objAgent.Init();
                agents.Add(objAgent);
            }
        }

        private void Cleanup()  //Al terminar la simulacion limpio la pantalla de los agentes, calculo sus puntajes y creo la nueva generacion
        {
            lastAvg = 0;
            foreach (Agent a in agents)
            {
                a.CalculateScore();  //Calculo el puntaje de cada agente
                lastAvg += a.GetScore();
            }
            lastAvg = lastAvg / agents.Count;  //Calculo el promedio de toda la generacion

            if (method == Method.Genetic)
			{                                //Mando a evolucionar a todos los agentes (Seleccion de la elite, ruleta, reproduccion, mutacion, etc)
                Chromosome[] newChromosomes = GeneticAlgorithm.Get().Evolve(agents.ToArray(), eliteProportion, mutationChance, mutationThreshold, genesPerChromosome);
                for (int i = 0; i < agents.Count; i++)
                {
					Destroy(agents[i].gameObject); //Destruyo la generacion vieja
                }
                agents.Clear();
				for (int i = 0; i < agentsPerGeneration; i++)  //Inicializo todos los agentes, les asigno su metodo de simulacion y los activo en escena
                {
                    GameObject objRef = Instantiate(agent, startPosition.position, Quaternion.identity);
                    Agent objAgent = objRef.GetComponent<Agent>();
                    objAgent.method = Method.Genetic;
                    objAgent.chromosome = newChromosomes[i];
                    objAgent.Init();
                    agents.Add(objAgent);
                    objAgent.gameObject.SetActive(false);
                }
            }
            else if (method == Method.NeuronalNetwork)
			{                                            //Mando a evolucionar a todos los agentes (Seleccion de la elite, ruleta, reproduccion, mutacion, etc)
                NeuronalNetworks.Network[] newNetworks = GeneticAlgorithm.Get().Evolve(agents.ToArray(), eliteProportion, mutationChance, mutationThreshold);
                for (int i = 0; i < agents.Count; i++)
                {
                    Destroy(agents[i].gameObject);  //Destruyo la generacion vieja
                }
                agents.Clear();
                for (int i = 0; i < agentsPerGeneration; i++)  //Inicializo todos los agentes, les asigno su metodo de simulacion y los activo en escena
                {
                    GameObject objRef = Instantiate(agent, startPosition.position, Quaternion.identity);
                    Agent objAgent = objRef.GetComponent<Agent>();
                    objAgent.method = Method.NeuronalNetwork;
                    objAgent.network = newNetworks[i];
                    objAgent.Init();
                    agents.Add(objAgent);
                    objAgent.gameObject.SetActive(false);
                }
            }

            simulationInProgress = false;
            simulationTimer = 0;
        }
    }
}
