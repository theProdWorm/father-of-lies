using System;
using Unity.Behavior;
using UnityEngine;
using Unity.Properties;

#if UNITY_EDITOR
[CreateAssetMenu(menuName = "Behavior/Event Channels/ChargePrep")]
#endif
[Serializable, GeneratePropertyBag]
[EventChannelDescription(name: "ChargePrep", message: "Agent started/finished prepping charge", category: "Events", id: "07ce1ca8a04324326cd8b7441cf9362c")]
public sealed partial class ChargePrep : EventChannel { }

