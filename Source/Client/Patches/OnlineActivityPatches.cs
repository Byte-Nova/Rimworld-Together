using HarmonyLib;
using Shared;
using Verse;
using Verse.AI;
using static Shared.CommonEnumerators;

namespace GameClient
{
    [HarmonyPatch(typeof(Pawn_JobTracker), nameof(Pawn_JobTracker.StartJob))]
    public static class PatchStartNewJob
    {
        [HarmonyPrefix]
        public static bool DoPre(Job newJob, Pawn ___pawn)
        {
            if (Network.state == ClientNetworkState.Disconnected) return true;
            if (SessionValues.currentRealTimeEvent == OnlineActivityType.None) return true;

            //Don't execute patch if map doesn't contain the pawn
            bool shouldPatch = false;
            if (OnlineActivityManagerHelper.factionPawns.Contains(___pawn)) shouldPatch = true;
            else if (OnlineActivityManagerHelper.nonFactionPawns.Contains(___pawn)) shouldPatch = true;

            if (!shouldPatch) return true;
            else
            {
                if (OnlineActivityManagerHelper.factionPawns.Contains(___pawn))
                {
                    //This is our pawn and we prepare the packet for the other player

                    return true;
                }

                else
                {
                    //This is not our pawn and we shouldn't handle him from here

                    return false;
                }
            }
        }
    }

    [HarmonyPatch(typeof(Thing), nameof(Thing.SpawnSetup))]
    public static class PatchCreateThing
    {
        [HarmonyPrefix]
        public static bool DoPre(Map map, Thing __instance)
        {
            if (Network.state == ClientNetworkState.Disconnected) return true;
            if (SessionValues.currentRealTimeEvent == OnlineActivityType.None) return true;
            if (!OnlineActivityManagerHelper.isActivityReady) return true;
            if (OnlineActivityManagerHelper.CheckIfIgnoreThingSync(__instance)) return true;

            if (!OnlineActivityManagerHelper.CheckInverseIfShouldPatch(__instance, true, true, true)) return true;
            else
            {
                if (OnlineActivityManagerHelper.isHost)
                {
                    OnlineActivityData OnlineActivityData = new OnlineActivityData();
                    OnlineActivityData._stepMode = OnlineActivityStepMode.Create;
                    OnlineActivityData._creationOrder = OnlineManagerOrders.CreateCreationOrder(__instance);

                    Packet packet = Packet.CreatePacketFromObject(nameof(OnlineActivityManager), OnlineActivityData);
                    Network.listener.EnqueuePacket(packet);

                    //KEEP ALWAYS AS AT THE BOTTOM AS POSSIBLE
                    OnlineActivityManagerHelper.AddThingToMap(__instance, Hasher.GetHashFromString(__instance.ThingID));
                    return true;
                }

                else
                {
                    // IF COMING FROM HOST
                    if (OnlineActivityManagerHelper.queuedThing == __instance)
                    {
                        OnlineActivityManagerHelper.AddThingToMap(__instance, OnlineActivityManagerHelper.queuedHash);
                        return true;
                    }

                    // IF PLAYER ASKING FOR
                    else return false;
                }
            }
        }
    }

    [HarmonyPatch(typeof(Thing), nameof(Thing.Destroy))]
    public static class PatchDestroyThing
    {
        [HarmonyPrefix]
        public static bool DoPre(Thing __instance)
        {
            if (Network.state == ClientNetworkState.Disconnected) return true;
            if (SessionValues.currentRealTimeEvent == OnlineActivityType.None) return true;
            if (!OnlineActivityManagerHelper.isActivityReady) return true;
            if (OnlineActivityManagerHelper.CheckIfIgnoreThingSync(__instance)) return true;

            if (!OnlineActivityManagerHelper.CheckIfShouldPatch(__instance, true, true, true)) return true;
            else
            {
                if (OnlineActivityManagerHelper.isHost)
                {
                    OnlineActivityData onlineActivityData = new OnlineActivityData();
                    onlineActivityData._stepMode = OnlineActivityStepMode.Destroy;
                    onlineActivityData._destructionOrder = OnlineManagerOrders.CreateDestructionOrder(__instance);

                    Packet packet = Packet.CreatePacketFromObject(nameof(OnlineActivityManager), onlineActivityData);
                    Network.listener.EnqueuePacket(packet);

                    //KEEP ALWAYS AS AT THE BOTTOM AS POSSIBLE
                    OnlineActivityManagerHelper.RemoveThingFromMap(__instance);
                    return true;
                }

                else
                {
                    // IF COMING FROM HOST
                    if (OnlineActivityManagerHelper.queuedThing == __instance)
                    {
                        OnlineActivityManagerHelper.RemoveThingFromMap(__instance);
                        return true;
                    }

                    // IF PLAYER ASKING FOR
                    else return false;
                }
            }
        }
    }

    [HarmonyPatch(typeof(Thing), nameof(Thing.TakeDamage))]
    public static class PatchApplyDamage
    {
        [HarmonyPrefix]
        public static bool DoPre(DamageInfo dinfo, Thing __instance, ref DamageWorker.DamageResult __result)
        {
            if (Network.state == ClientNetworkState.Disconnected) return true;
            if (SessionValues.currentRealTimeEvent == OnlineActivityType.None) return true;
            if (!OnlineActivityManagerHelper.isActivityReady) return true;

            if (!OnlineActivityManagerHelper.CheckIfShouldPatch(__instance, true, true, true)) return true;
            else
            {
                if (OnlineActivityManagerHelper.isHost)
                {
                    OnlineActivityData onlineActivityData = new OnlineActivityData();
                    onlineActivityData._stepMode = OnlineActivityStepMode.Damage;
                    onlineActivityData._damageOrder = OnlineManagerOrders.CreateDamageOrder(dinfo, __instance);

                    Packet packet = Packet.CreatePacketFromObject(nameof(OnlineActivityManager), onlineActivityData);
                    Network.listener.EnqueuePacket(packet);
                    return true;
                }

                else
                {
                    //IF COMING FROM HOST

                    if (OnlineActivityManagerHelper.queuedThing == __instance)
                    {
                        OnlineActivityManagerHelper.SetThingQueue(null);
                        return true;
                    }

                    //IF PLAYER ASKING FOR

                    else
                    {
                        //Create empty DamageWorker.DamageResult so the functions expecting it don't freak out
                        __result = new DamageWorker.DamageResult();
                        __result.totalDamageDealt = 0f;
                        return false;
                    }
                }
            }
        }
    }
}
