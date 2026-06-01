using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;

public static class WorldEncounterGenerator
{
    [MenuItem("Tools/Generate World Encounters")]
    public static void Generate()
    {
        string folderPath = "Assets/ScriptableObjects/World/NewWorld/Encounters/World";
        
        if (!AssetDatabase.IsValidFolder(folderPath))
        {
            string[] split = folderPath.Split('/');
            string currentPath = split[0];
            for (int i = 1; i < split.Length; i++)
            {
                string nextPath = currentPath + "/" + split[i];
                if (!AssetDatabase.IsValidFolder(nextPath))
                {
                    AssetDatabase.CreateFolder(currentPath, split[i]);
                }
                currentPath = nextPath;
            }
        }

        // 1. Muggdrassil (REQUIRES: flag_corteza)
        CreateEncounter(folderPath, "World_Muggdrassil", "enc_world_muggdrassil", "El Arbol de la Vida",
            "Llegas a las inmensas raices de Muggdrassil. El anciano druida te mira con desden: 'El arbol sangra desde que profanaron el Mitonar. No otorgare la fuerza de la bestia a quien no traiga de vuelta la Corteza Primigenia'.",
            new List<EncounterOptionDefinition>
            {
                new EncounterOptionDefinition
                {
                    optionText = "Entregar la Corteza Sagrada del Mitonar.",
                    requiredFlag = "flag_corteza",
                    statToCheck = StatType.MV, difficulty = 0,
                    successText = "Las lagrimas brotan de los ojos del druida al recolocar la corteza en el tronco. Agradecido, traza una runa verde en tu frente. Sientes como una fuerza animal primigenia invade tus musculos (+5 Fuerza Bruta).",
                    successEffects = new[] { 
                        new EncounterEffect { type = EncounterEffectType.RemoveFlag, flagName = "flag_corteza" },
                        new EncounterEffect { type = EncounterEffectType.AddPermanentStat, statToBoost = StatType.BF, statBoostValue = 5 },
                        new EncounterEffect { type = EncounterEffectType.SetFlag, flagName = "flag_done_muggdrassil" }
                    }
                },
                new EncounterOptionDefinition
                {
                    optionText = "No tengo la corteza. Regresare cuando la encuentre.",
                    forbiddenFlag = "flag_corteza",
                    statToCheck = StatType.MV, difficulty = 0,
                    successText = "El druida asiente en silencio. Debes buscar en las ruinas del Mitonar.",
                    successEffects = new EncounterEffect[0]
                }
            });

        // 2. Chateau Blanc
        CreateEncounter(folderPath, "World_ChateauBlanc", "enc_world_chateaublanc", "Bodegas Chateau Blanc",
            "Las imponentes bodegas de Chateau Blanc se alzan entre vinedos perfectos. El maestro bodeguero te ofrece su Cosecha Diamante, un vino capaz de corromper voluntades.",
            new List<EncounterOptionDefinition>
            {
                new EncounterOptionDefinition
                {
                    optionText = "Comprar la Cosecha Diamante (-50 Monedas)",
                    forbiddenFlag = "flag_has_wine",
                    statToCheck = StatType.MV, difficulty = 0,
                    successText = "Desembolsas el oro y guardas las fragiles botellas. Seguro que hay criadores de caballos o nobles dispuestos a pagar fortunas o favores por esto.",
                    successEffects = new[] { 
                        new EncounterEffect { type = EncounterEffectType.LoseMoney, value = 50 },
                        new EncounterEffect { type = EncounterEffectType.SetFlag, flagName = "flag_has_wine" }
                    }
                },
                new EncounterOptionDefinition
                {
                    optionText = "Ignorar la oferta y marcharme.",
                    statToCheck = StatType.MV, difficulty = 0,
                    successText = "El alcohol y la justa no son buenos aliados.",
                    successEffects = new EncounterEffect[0]
                }
            });

        // 3. El Mitonar (GIVES: flag_corteza)
        CreateEncounter(folderPath, "World_Mitonar", "enc_world_mitonar", "Las Ruinas de Mitonar",
            "Te adentras en la neblina de la ciudad destruida. Entre los escombros del antiguo templo, algo palpita con magia antigua.",
            new List<EncounterOptionDefinition>
            {
                new EncounterOptionDefinition
                {
                    optionText = "Usar mi precision para desenterrar el objeto sin derrumbar el templo.",
                    statToCheck = StatType.M, difficulty = 15,
                    successText = "Logras apartar las pesadas losas de marmol. En el centro de un relicario polvoriento encuentras la 'Corteza del Arbol Primigenio'. Notas que la madera esta extranamente caliente.",
                    failureText = "Das un mal paso y una columna entera se derrumba sobre ti. Pasas dias curando tus fracturas.",
                    successEffects = new[] { 
                        new EncounterEffect { type = EncounterEffectType.AddMoney, value = 50 },
                        new EncounterEffect { type = EncounterEffectType.SetFlag, flagName = "flag_corteza" }
                    },
                    failureEffects = new[] { new EncounterEffect { type = EncounterEffectType.AddDays, value = 3 } }
                }
            });

        // 4. Puertas del Dragon
        CreateEncounter(folderPath, "World_PuertasDragon", "enc_world_puertasdragon", "Las Puertas del Dragon",
            "Un antiguo portal magico hecho de obsidiana ruge de energia arcana. El vortice parece conectar con el sur.",
            new List<EncounterOptionDefinition>
            {
                new EncounterOptionDefinition
                {
                    optionText = "Cruzar el portal a ciegas.",
                    statToCheck = StatType.MV, difficulty = 0,
                    successText = "El espacio se pliega a tu alrededor y apareces de golpe en Los Tumulos.",
                    successEffects = new[] { new EncounterEffect { type = EncounterEffectType.MoveToNode, targetNodeId = "node_tumulos" } }
                },
                new EncounterOptionDefinition
                {
                    optionText = "Dar media vuelta.",
                    statToCheck = StatType.MV, difficulty = 0,
                    successText = "Prefieres que tus pies toquen tierra firme.",
                    successEffects = new EncounterEffect[0]
                }
            });

        // 5. Tumulos
        CreateEncounter(folderPath, "World_Tumulos", "enc_world_tumulos", "Los Antiguos Tumulos",
            "Las tumbas de los primeros reyes emanan una magia resonante, identica a la de la Puerta del Dragon. Sientes que podrias canalizarla.",
            new List<EncounterOptionDefinition>
            {
                new EncounterOptionDefinition
                {
                    optionText = "Intentar canalizar la magia para abrir una falla de retorno.",
                    statToCheck = StatType.M, difficulty = 20,
                    successText = "Logras concentrar la energia dispersa. El aire se rasga y cruzas al instante hacia Las Puertas del Dragon.",
                    failureText = "La magia nigromantica rechaza tu control. Te drena la energia vital y duermes durante dias.",
                    successEffects = new[] { new EncounterEffect { type = EncounterEffectType.MoveToNode, targetNodeId = "node_puertasdragon" } },
                    failureEffects = new[] { new EncounterEffect { type = EncounterEffectType.AddDays, value = 3 } }
                },
                new EncounterOptionDefinition
                {
                    optionText = "Dejar los espiritus en paz.",
                    statToCheck = StatType.MV, difficulty = 0,
                    successText = "Has venido a justar, no a profanar tumbas.",
                    successEffects = new EncounterEffect[0]
                }
            });

        // 6. Ruinas de Thurich (GIVES: flag_piedraguardian)
        CreateEncounter(folderPath, "World_Thurich", "enc_world_thurich", "El Constructor Solitario",
            "Un anciano solitario carga enormes bloques de piedra, intentando reconstruir la antigua Thurich, arrasada por los dragones.",
            new List<EncounterOptionDefinition>
            {
                new EncounterOptionDefinition
                {
                    optionText = "Ayudarle a cargar los bloques pesados.",
                    statToCheck = StatType.BF, difficulty = 18,
                    successText = "Trabajas duro durante dias. El anciano resulta ser el guardian de una orden caida y, en agradecimiento, te entrega la reliquia de su orden: la 'Piedra del Guardian'.",
                    failureText = "Te excedes intentando mover una losa colosal y te lesionas gravemente la espalda.",
                    successEffects = new[] { 
                        new EncounterEffect { type = EncounterEffectType.AddDays, value = 2 },
                        new EncounterEffect { type = EncounterEffectType.AddMoney, value = 50 },
                        new EncounterEffect { type = EncounterEffectType.SetFlag, flagName = "flag_piedraguardian" }
                    },
                    failureEffects = new[] { new EncounterEffect { type = EncounterEffectType.AddDays, value = 3 } }
                },
                new EncounterOptionDefinition
                {
                    optionText = "Seguir mi camino.",
                    statToCheck = StatType.MV, difficulty = 0,
                    successText = "Un solo hombre jamas reconstruira una ciudad.",
                    successEffects = new EncounterEffect[0]
                }
            });

        // 7. Hacienda Maulkin (REQUIRES: flag_fustainfernal)
        CreateEncounter(folderPath, "World_Maulkin", "enc_world_maulkin", "El Secreto del Jinete",
            "El legendario criador de la Hacienda Maulkin te mira severamente. 'Solo comparto mis secretos de monta con quien me demuestre que domina a los caballos demoniacos. Traeme la fusta negra del sur y hablaremos'.",
            new List<EncounterOptionDefinition>
            {
                new EncounterOptionDefinition
                {
                    optionText = "Mostrarle la Fusta Infernal encontrada en la Boca del Demonio.",
                    requiredFlag = "flag_fustainfernal",
                    statToCheck = StatType.MV, difficulty = 0,
                    successText = "El criador palidece de respeto. Te lleva a las caballerizas privadas y te ensena los trucos magistrales de los jinetes ancestrales. A partir de ahora, cabalgaras como el mismisimo viento (+5 Velocidad).",
                    successEffects = new[] { 
                        new EncounterEffect { type = EncounterEffectType.RemoveFlag, flagName = "flag_fustainfernal" },
                        new EncounterEffect { type = EncounterEffectType.AddPermanentStat, statToBoost = StatType.V, statBoostValue = 5 },
                        new EncounterEffect { type = EncounterEffectType.SetFlag, flagName = "flag_done_maulkin" }
                    }
                },
                new EncounterOptionDefinition
                {
                    optionText = "Me marchare a buscar esa fusta a la Boca del Demonio.",
                    forbiddenFlag = "flag_fustainfernal",
                    statToCheck = StatType.MV, difficulty = 0,
                    successText = "Que los dioses te acompanen en tu viaje al sur.",
                    successEffects = new EncounterEffect[0]
                }
            });

        // 8. Puente Negro
        CreateEncounter(folderPath, "World_PuenteNegro", "enc_world_puentenegro", "El Desafio del Puente",
            "Un imponente Caballero Negro bloquea el puente de piedra. 'Ningun mortal cruza este puente sin derramar sangre... o probar su valia en la liza'.",
            new List<EncounterOptionDefinition>
            {
                new EncounterOptionDefinition
                {
                    optionText = "Aceptar el desafio.",
                    statToCheck = StatType.MV, difficulty = 0,
                    successText = "El caballero asiente lentamente. Abres la marcha hacia el campo del honor. Prepara tus armas para la justa mas dura de tu vida.",
                    successEffects = new[] { new EncounterEffect { type = EncounterEffectType.MoveToNode, targetNodeId = "node_justa_negra" } }
                },
                new EncounterOptionDefinition
                {
                    optionText = "Huir y buscar otro camino a campo traviesa.",
                    statToCheck = StatType.MV, difficulty = 0,
                    successText = "El caballero rie a carcajadas bajo su yelmo mientras te alejas por el barro, perdiendo tiempo valioso.",
                    successEffects = new[] { new EncounterEffect { type = EncounterEffectType.AddDays, value = 1 } }
                }
            });

        // 9. Bosque del Norte (REQUIRES: flag_piedraguardian)
        CreateEncounter(folderPath, "World_BosqueNorte", "enc_world_bosquenorte", "El Juicio del Espiritu",
            "Te adentras en las sombras del denso bosque. De repente, la arboleda se agita violentamente. Un espiritu ancestral en forma de bestia de madera te corta el paso.",
            new List<EncounterOptionDefinition>
            {
                new EncounterOptionDefinition
                {
                    optionText = "Alzar la Piedra del Guardian (Ruinas de Thurich).",
                    requiredFlag = "flag_piedraguardian",
                    statToCheck = StatType.MV, difficulty = 0,
                    successText = "Al ver la Piedra, la furia del espiritu se aplaca. Te envuelve en un manto de luz esmeralda, otorgandote la resiliencia eterna del bosque (+5 Balance de Defensa).",
                    successEffects = new[] { 
                        new EncounterEffect { type = EncounterEffectType.RemoveFlag, flagName = "flag_piedraguardian" },
                        new EncounterEffect { type = EncounterEffectType.AddPermanentStat, statToBoost = StatType.BB, statBoostValue = 5 },
                        new EncounterEffect { type = EncounterEffectType.SetFlag, flagName = "flag_done_bosquenorte" }
                    }
                },
                new EncounterOptionDefinition
                {
                    optionText = "Huir del bosque antes de que te aplaste.",
                    forbiddenFlag = "flag_piedraguardian",
                    statToCheck = StatType.MV, difficulty = 0,
                    successText = "Consigues escapar por los pelos, magullado y agotado. Parece que necesitas un artefacto de las antiguas ruinas del este para apaciguarlo.",
                    successEffects = new[] { new EncounterEffect { type = EncounterEffectType.AddDays, value = 1 } }
                }
            });

        // 10. Valle de los Reyes (GIVES: flag_emblemaviento)
        CreateEncounter(folderPath, "World_ValleReyes", "enc_world_vallereyes", "El Valle Subterraneo",
            "Descubres una red de tuneles secretos plagados de trampas de virotes. En el centro de la sala mas profunda brilla un altar.",
            new List<EncounterOptionDefinition>
            {
                new EncounterOptionDefinition
                {
                    optionText = "Confiar en mi velocidad para esquivar los virotes.",
                    statToCheck = StatType.V, difficulty = 18,
                    successText = "Esquivas la rafaga mortal y alcanzas el altar. Arrebatas el 'Emblema del Viento Antiguo', pero el suelo cede y caes al abismo... ¡hasta escupirte en la Boca del Demonio!",
                    failureText = "Un virote te alcanza la pierna. Te pierdes sangrando en la oscuridad durante semanas, consumido por la fiebre.",
                    successEffects = new[] { 
                        new EncounterEffect { type = EncounterEffectType.SetFlag, flagName = "flag_emblemaviento" },
                        new EncounterEffect { type = EncounterEffectType.MoveToNode, targetNodeId = "node_bocademonio" }
                    },
                    failureEffects = new[] { new EncounterEffect { type = EncounterEffectType.AddDays, value = 5 } }
                }
            });

        // 11. Boca del Demonio (GIVES: flag_fustainfernal)
        CreateEncounter(folderPath, "World_BocaDemonio", "enc_world_bocademonio", "Garganta Demoniaca",
            "Una sima infernal de paredes escarpadas humea azufre bajo tus pies. Distingues el esqueleto de un corcel de pesadilla y su jinete en el fondo.",
            new List<EncounterOptionDefinition>
            {
                new EncounterOptionDefinition
                {
                    optionText = "Descender escalando a pulso puro.",
                    statToCheck = StatType.BF, difficulty = 18,
                    successText = "Tus musculos arden, pero llegas abajo. Entre los huesos carbonizados hallas la legendaria 'Fusta Infernal'. Un geiser de presion te expulsa... ¡hasta el Valle de los Reyes!",
                    failureText = "Tus manos resbalan. Respiras los vapores toxicos y caes varios metros. Tardas dias en encontrar una salida, medio muerto.",
                    successEffects = new[] { 
                        new EncounterEffect { type = EncounterEffectType.SetFlag, flagName = "flag_fustainfernal" },
                        new EncounterEffect { type = EncounterEffectType.MoveToNode, targetNodeId = "node_vallereyes" }
                    },
                    failureEffects = new[] { new EncounterEffect { type = EncounterEffectType.AddDays, value = 4 } }
                }
            });

        // 12. Tribu del Cielo (REQUIRES: flag_emblemaviento)
        CreateEncounter(folderPath, "World_TribuCielo", "enc_world_tribucielo", "El Puente de las Nubes",
            "El temible Caudillo Alado bloquea el puente levadizo que da acceso a las montanas mas altas. 'Solo los Senores del Viento pueden pasar', ruge, cruzando las lanzas de sus guerreros.",
            new List<EncounterOptionDefinition>
            {
                new EncounterOptionDefinition
                {
                    optionText = "Elevar orgullosamente el Emblema del Viento del Valle de los Reyes.",
                    requiredFlag = "flag_emblemaviento",
                    statToCheck = StatType.MV, difficulty = 0,
                    successText = "Las lanzas se apartan de inmediato. El Caudillo se arrodilla ante ti. Un chaman se acerca y te imbuye con la vision del aguila y la precision del halcon (+5 Balance de Lanza).",
                    successEffects = new[] { 
                        new EncounterEffect { type = EncounterEffectType.RemoveFlag, flagName = "flag_emblemaviento" },
                        new EncounterEffect { type = EncounterEffectType.AddPermanentStat, statToBoost = StatType.BL, statBoostValue = 5 },
                        new EncounterEffect { type = EncounterEffectType.SetFlag, flagName = "flag_done_tribucielo" }
                    }
                },
                new EncounterOptionDefinition
                {
                    optionText = "Me retirare a buscar la forma de probar que soy un Senor del Viento.",
                    forbiddenFlag = "flag_emblemaviento",
                    statToCheck = StatType.MV, difficulty = 0,
                    successText = "El caudillo asiente. 'Busca en los laberintos enterrados de los valles del sur'.",
                    successEffects = new EncounterEffect[0]
                }
            });

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("World encounters generated successfully!");
    }

    private static void CreateEncounter(string folder, string fileName, string encounterId, string title, string description, List<EncounterOptionDefinition> options)
    {
        string path = $"{folder}/{fileName}.asset";
        RandomEncounterDefinition asset = AssetDatabase.LoadAssetAtPath<RandomEncounterDefinition>(path);
        
        if (asset == null)
        {
            asset = ScriptableObject.CreateInstance<RandomEncounterDefinition>();
            AssetDatabase.CreateAsset(asset, path);
        }

        asset.encounterId = encounterId;
        asset.title = title;
        asset.description = description;
        asset.options = options;

        EditorUtility.SetDirty(asset);
    }
}
