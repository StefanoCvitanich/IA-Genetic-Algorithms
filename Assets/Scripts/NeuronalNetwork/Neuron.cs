using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace IA.NeuronalNetworks
{
    public struct Neuron
    {
        public float activation;
        public float bias;
        public float biasWeight;
        public Link[] links;
    }
}
