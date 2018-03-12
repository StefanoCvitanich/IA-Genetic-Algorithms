using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace IA.NeuronalNetworks
{
    public class WrongInputQuantException : System.Exception  
    {
        public WrongInputQuantException(string message) : base(message) 
        {
            Debug.LogError(message);
        }
    }
																		//Excepciones para distintos tipos de errores
    public class InvalidMergeException : System.Exception
    {
        public InvalidMergeException(string message) : base(message) 
        {
            Debug.LogError(message);
        }
    }

    public class Network
    {
        public static Network MergeNetworks(Network a, Network b)  //Chequeo que pueda mezclar dos redes diferentes
        {
            if (a.inputLayer.neurons.Length != b.inputLayer.neurons.Length ||
               a.hiddenLayers.Length != b.hiddenLayers.Length ||
               a.outputLayer.neurons.Length != b.outputLayer.neurons.Length)
            {
                throw new InvalidMergeException("Cannot merge networks with different layer sizes.");
            }

            Network output = new Network();  //Capa resultante de la union de dos capas
            if (a.hiddenLayers.Length > 0)
                output.Init(a.inputLayer.neurons.Length, a.hiddenLayers[0].neurons.Length, a.hiddenLayers.Length, a.outputLayer.neurons.Length, a.globalBias); //Inicializo la red con capas ocultas
            else
                output.Init(a.inputLayer.neurons.Length, 0, 0, a.outputLayer.neurons.Length, a.globalBias);  //Inicializo la red sin capas ocultas

            for (int i = 0; i < output.hiddenLayers.Length; i++)
            {
                for (int j = 0; j < output.hiddenLayers[i].neurons.Length; j++)
                {
                    if (j % 2 == 0)
                        output.hiddenLayers[i].neurons[j].biasWeight = a.hiddenLayers[i].neurons[j].biasWeight; 
					else                                                                                       //Heredo a la red hija la primera mitad de los pesos de los bias de la madre y la segunda mitad de los pesos de los bias del padre (en las capas ocultas)
                        output.hiddenLayers[i].neurons[j].biasWeight = b.hiddenLayers[i].neurons[j].biasWeight;
                    for (int k = 0; k < output.hiddenLayers[i].neurons[j].links.Length; k++)
                    {
                        if (k % 2 == 0)
                            output.hiddenLayers[i].neurons[j].links[k].weight = a.hiddenLayers[i].neurons[j].links[k].weight;
						else                                                                                                  //Heredo a la red hija la primera mitad de los pesos de los links de la madre y la segunda mitad de los pesos de los links del padre (en las capas ocultas)                                                                                               
                            output.hiddenLayers[i].neurons[j].links[k].weight = b.hiddenLayers[i].neurons[j].links[k].weight;
                    }
                }
            }
            for (int i = 0; i < output.outputLayer.neurons.Length; i++)
            {
                if (i % 2 == 0)
                    output.outputLayer.neurons[i].biasWeight = a.outputLayer.neurons[i].biasWeight;
				else                                                                                //Heredo a la red hija la primera mitad de los pesos de los bias de la madre y la segunda mitad de los pesos de los bias del padre (en la Output layer)
                    output.outputLayer.neurons[i].biasWeight = b.outputLayer.neurons[i].biasWeight;
                for (int j = 0; j < output.outputLayer.neurons[i].links.Length; j++)
                {
                    if (j % 2 == 0)
                        output.outputLayer.neurons[i].links[j].weight = a.outputLayer.neurons[i].links[j].weight;
					else                                                                                          //Heredo a la red hija la primera mitad de los pesos de los links de la madre y la segunda mitad de los pesos de los links del padre (en la Output layer)
                        output.outputLayer.neurons[i].links[j].weight = b.outputLayer.neurons[i].links[j].weight;
                }
            }

            return output;
        }

        private Layer inputLayer;
        private Layer[] hiddenLayers;
        private Layer outputLayer;
        private float globalBias;

        public void RandomizeWeights(float min, float max, float biasMin, float biasMax)
        {
            for (int i = 0; i < hiddenLayers.Length; i++)
            {
                for (int j = 0; j < hiddenLayers[i].neurons.Length; j++)
                {
                    hiddenLayers[i].neurons[j].biasWeight = Random.Range(biasMin, biasMax);  //Asigno un valor aleatorio a los pesos de los bias de las neuronas
                    for(int k = 0; k < hiddenLayers[i].neurons[j].links.Length; k++)
                    {
                        hiddenLayers[i].neurons[j].links[k].weight = Random.Range(min, max); //Asigno un peso aleatorio a los links que tiene una neurona con cada neurona de la capa anterior
                    }
                }
            }

            for (int i = 0; i < outputLayer.neurons.Length; i++)
            {
                outputLayer.neurons[i].biasWeight = Random.Range(biasMin, biasMax);  // Lo mismo de antes pero con la output layer
                for (int j = 0; j < outputLayer.neurons[i].links.Length; j++)
                {
                    outputLayer.neurons[i].links[j].weight = Random.Range(min, max); //Otra vez lo mismo que con las hidden layers
                }
            }
        }

        public void Init(int inputLayerSize, int hiddenLayerSize, int hiddenLayerQuant, int outputLayerSize, float linkBias)
        {
            globalBias = linkBias;
            inputLayer = new Layer(inputLayerSize, linkBias);  //Inicializo una nueva capa Input con lo valores pasados por parametro
            hiddenLayers = new Layer[hiddenLayerQuant];
            for (int i = 0; i < hiddenLayers.Length; i++)
            {
				hiddenLayers[i] = new Layer(hiddenLayerSize, linkBias); //Inicializo nuevas capas ocultast con lo valores pasados por parametro
            }
			outputLayer = new Layer(outputLayerSize, linkBias); //Inicializo una nueva capa Output con lo valores pasados por parametro

            if (hiddenLayers.Length > 1)
            {
                inputLayer.next = hiddenLayers[0];
                for (int i = 0; i < hiddenLayers.Length - 1; i++)
                {
                    hiddenLayers[i].next = hiddenLayers[i + 1];  //Si hay mas de una capa oculta en la red, le asigno cual es la siguiente capa
                }
                hiddenLayers[hiddenLayers.Length - 1].next = outputLayer; //A la ultima capa oculta le asigno la Output como su siguiente capa
            }
            else if (hiddenLayers.Length == 1)
            {
                inputLayer.next = hiddenLayers[0];
                hiddenLayers[0].next = outputLayer;  //Lo mismo que antes pero para una sola capa oculta
            }
            else
            {
                inputLayer.next = outputLayer; // Si no hay capas ocultas le asigno a la Input la Output como la siguente capa
            }

            inputLayer.CreateLinks();  //Le creo los links a la Input Layer
            if (hiddenLayers.Length > 0)
            {
                foreach (Layer l in hiddenLayers)
                {
                    l.CreateLinks();  //Si tengo hidden layers, les creo los links
                }
            }
        }

        public float[] Process(float[] inputs)
        {
            if (inputs.Length != inputLayer.neurons.Length)  //Si la cant de inputs es mayor a la cant de neuronas en la input layer, tiro error
            {
                throw new WrongInputQuantException("Number of inputs must match the number of neurons in the Input Layer."); 
            }
            for(int i = 0; i < inputLayer.neurons.Length; i++)
            {
                inputLayer.neurons[i].activation = inputs[i]; //Le asigno los valores de activacion a las neuronas de la input layer a partir de los inputs
            }
            float[] output = new float[outputLayer.neurons.Length];
            
            if (hiddenLayers.Length > 0)
            {
                foreach (Layer l in hiddenLayers)
                {
                    l.ProcessActivationValues();  //Calculo las activaciones de todas las neuronas de cada capa
                }
            }
			outputLayer.ProcessActivationValues();  //Calculo los valores de activacion de la ultima capa (Output)

            for (int i = 0; i < outputLayer.neurons.Length; i++)
            {
                output[i] = outputLayer.neurons[i].activation;  //Guardo los valores obtenidos de la capa Output
            }
            return output;
        }
    }
}