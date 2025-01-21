using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TapEmpire.Services
{
    public enum PurchaseFailureReason
    {
        PurchasingUnavailable,
        ExistingPurchasePending,
        ProductUnavailable,
        SignatureInvalid,
        UserCancelled,
        PaymentDeclined,
        DuplicateTransaction,
        Unknown
    }
}