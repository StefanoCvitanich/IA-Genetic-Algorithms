using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace IA.NeuronalNetworks
{
    public class Layer
    {
        public Layer(int size, float linkBias)  //Inicializo Layer capa
        {
            neurons = new Neuron[size];
            for (int i = 0; i < size; i++)
            {
                neurons[i] = new Neuron();
                neurons[i].bias = linkBias;
            }
        }

        // No usar con la Output Layer
        public void CreateLinks() //Creo las conecciones entre una capa y la siguiente. Como la Output layer no tiene siguiente, no la uso con ella
        {
            for(int i = 0; i < next.neurons.Length; i++)
            {
                next.neurons[i].links = new Link[neurons.Length];
                for(int j = 0; j < neurons.Length; j++)
                {
                    next.neurons[i].links[j] = new Link();
                    next.neurons[i].links[j].from = neurons[j];
                    next.neurons[i].links[j].weight = 0;
                }
            }
        }

        // No usar con la Input Layer
        public void ProcessActivationValues() //Calculo valores de activacion usando los datos de la capa anterior. Como la Input Layer no tiene una capa que la preceda, no la uso con ella
        {
            for(int i = 0; i < neurons.Length; i++) //Tomo de a una neurona y le calculo su activacion
            {
                float weightedSum = 0;
                for(int j = 0; j < neurons[i].links.Length; j++)
                {
                    weightedSum += neurons[i].links[j].from.activation * neurons[i].links[j].weight; //Hago la sumatoria de valores de activacion multiplicados por su peso
                }
                weightedSum -= neurons[i].bias * neurons[i].biasWeight; //al valor obtenido le sumo el bias multiplicado por su peso
                neurons[i].activation = MathS.Sigmoid(weightedSum) * 2; //al valor obtenido le aplico la funcion Sigmoide para obtener una valor de activacion entre 0 y 1
            }
        }

        public Neuron[] neurons;
        public Layer next = null;
    }
}
