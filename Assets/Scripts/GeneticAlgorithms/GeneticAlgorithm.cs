using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace IA.GeneticAlgorithm
{
    public class GeneticAlgorithm
    {
        private static GeneticAlgorithm instance;
        public static GeneticAlgorithm Get()  //Singleton del algoritmo genetico
        {
            if (instance == null)
                instance = new GeneticAlgorithm();
            return instance;
        }

        //Seleccion de los supervivientes
        /*
        Se rescata la mitad que tuvo un mejor desempeño y se marca la elite dentro de ese grupo
        */
        private Dictionary<Agent, bool> PickSurvivors(List<Agent> agents, float eliteProportion)
        {
            Dictionary<Agent, bool> survivors = new Dictionary<Agent, bool>();
            agents.Sort((y, x) => x.GetScore().CompareTo(y.GetScore()));
            for(int i = 0; i < agents.Count / 2; i++)
            {
                if(i <= agents.Count / 2 * eliteProportion)
                {
                    survivors.Add(agents[i], true); //True si pertenece a la elite
                }
                else
                {
                    survivors.Add(agents[i], false); //False si NO pertenece a la elite
                }
            }
            return survivors;
        }

        //Ruleta
        /*
        Elije a las parejas que se van a reproducir (quien con quien)
        */
        Dictionary<Agent, Agent> Roulette(Dictionary<Agent, bool> survivors)
        {
            Dictionary<Agent, Agent> mates = new Dictionary<Agent, Agent>();
            int survivorCount = survivors.Count;

			List<Agent> agents = new List<Agent>();  //Hago una lista de agentes con los que sobrevivieron (la mejor mitad) y la lleno
            foreach (Agent a in survivors.Keys)
            {
                agents.Add(a);
            }
            while (agents.Count > 0) // Elijo a la pareja
            {
                int first = Random.Range(0, agents.Count);
                Agent f = agents[first];
                agents.RemoveAt(first);
                int second = Random.Range(0, agents.Count);
                Agent s = agents[second];
                agents.RemoveAt(second);
                mates.Add(f, s);
            }
            return mates;
        }

        //Reproduccion
        /*
        Genera nuevos cromosomas usando la mitad de cada padre. Cada pareja tiene dos hijos. Cada hijo obtiene mitades diferentres de los cromosomas
        */
        Chromosome[] Breed(Dictionary<Agent, bool> survivors, Dictionary<Agent, Agent> mates, int geneCount)
        {
            List<Chromosome> output = new List<Chromosome>();
            foreach (Agent a in survivors.Keys)
            {
                output.Add(a.chromosome);
            }
            
            foreach (KeyValuePair<Agent, Agent> mate in mates)
            {
                Chromosome first = new Chromosome();
                first.geneCount = geneCount;
                first.genes = new Gene[geneCount];
                for(int i = 0; i < geneCount; i++)
                {
                    if (i < geneCount / 2)
                    {
                        first.genes[i] = mate.Key.chromosome.genes[i]; //Primera mitad de la madre
                    }
                    else
                    {
                        first.genes[i] = mate.Value.chromosome.genes[i]; //Segunda mitad del padre
                    }
                }

                Chromosome second = new Chromosome();
                second.geneCount = geneCount;
                second.genes = new Gene[geneCount];
                for (int i = 0; i < geneCount; i++)
                {
                    if (i < geneCount / 2)
                    {
                        second.genes[i] = mate.Value.chromosome.genes[i]; //Primera mitad del padre
                    }
                    else
                    {
                        second.genes[i] = mate.Key.chromosome.genes[i];  //Segunda mitad de la madre
                    }
                }

                output.Add(first); //Primer hijo
                output.Add(second); //Segundo hijo
            }

            return output.ToArray();
        }

        NeuronalNetworks.Network[] Breed(Dictionary<Agent, bool> survivors, Dictionary<Agent, Agent> mates)
        {
            List<NeuronalNetworks.Network> output = new List<NeuronalNetworks.Network>();
            foreach (Agent a in survivors.Keys)
            {
                output.Add(a.network);
            }

			foreach (KeyValuePair<Agent, Agent> mate in mates)  //Por cada pareja creo dos redes hijas; dandoles distintas partes de los pesos de la red de cada padre
            {
                NeuronalNetworks.Network out1 = NeuronalNetworks.Network.MergeNetworks(mate.Key.network, mate.Value.network);
                NeuronalNetworks.Network out2 = NeuronalNetworks.Network.MergeNetworks(mate.Value.network, mate.Key.network);
                output.Add(out1);
                output.Add(out2);
            }

            return output.ToArray();
        }

        //Mutar
        /*
        Para cada generacion los agentes tienen una pequeña chance de mutar sus genes
        Se toman todos sus genes y se les modifica levemente la duracion de las acciones; todo de manera aleatoria
        */
        void Mutate(ref Chromosome[] chromosomes, float chance, float threshold)
        {
            foreach(Chromosome chromosome in chromosomes)
            {
                float r = Random.Range(0.0f, 1.0f);
                if(r <= chance)
                {
                    for(int i = 0; i < chromosome.geneCount; i++)
                    {
                        chromosome.genes[i].time += Random.Range(-threshold, threshold);
                    }
                }
            }
        }

        //Evolucionar
        /*
        Implementa los metodos arriba mencionados utilizando la generacion actual y produciendo una nueva
        */
        public Chromosome[] Evolve(Agent[] input, float eliteProportion, float mutationChance, float mutationThreshold, int genesPerChromosome)
        {
            
            List<Agent> agentList = new List<Agent>(input);
            Dictionary<Agent, bool> survivors = PickSurvivors(agentList, eliteProportion);
            
            Dictionary<Agent, Agent> mates = Roulette(survivors);

            Chromosome[] output = Breed(survivors, mates, genesPerChromosome);

            Mutate(ref output, mutationChance, mutationThreshold);

            return output;
        }

        public NeuronalNetworks.Network[] Evolve(Agent[] input, float eliteProportion, float mutationChance, float mutationTreshold)
        { //Sobrecarga de Evolucionar para Redes Neuronales

            List<Agent> agentList = new List<Agent>(input);
            Dictionary<Agent, bool> survivors = PickSurvivors(agentList, eliteProportion);

            Dictionary<Agent, Agent> mates = Roulette(survivors);

            NeuronalNetworks.Network[] output = Breed(survivors, mates);

            return output;
        }
    }
}
