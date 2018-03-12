using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace IA.GeneticAlgorithm
{
    public struct Chromosome
    {
		//Componentes del cromosoma
        public Gene[] genes;
        public int geneCount;
        public float score;
    }
}
