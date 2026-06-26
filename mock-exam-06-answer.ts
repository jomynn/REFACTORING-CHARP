// =============================================================================
// MOCK EXAM #06 — ANSWER KEY
// Smells fixed:
//   1. Mutates input       → returns new array [...cart.items, item] instead of mutating
//   2. Magic numbers       → TAX_RATE, BULK_THRESHOLD, BULK_DISCOUNT_RATE constants
//   3. DRY violation       → calculateSubtotal(items) extracted once, reused
//   4. Implicit any types  → CartItem, Cart, CheckoutResult interfaces defined
//   5. SRP violation       → CartService / CartValidator / DiscountPolicy / IPaymentGateway / IEmailService separated
//   6. Tight coupling      → IPaymentGateway + IEmailService injected via constructor
// =============================================================================

// ---------------------------------------------------------------------------
// Constants
// ---------------------------------------------------------------------------
const TAX_RATE = 0.07;
const BULK_THRESHOLD = 5;
const BULK_DISCOUNT_RATE = 0.15;

// ---------------------------------------------------------------------------
// Interfaces
// ---------------------------------------------------------------------------
interface CartItem {
  productId: string;
  name: string;
  price: number;
  qty: number;
}

interface Cart {
  customerId: string;
  items: readonly CartItem[];
}

interface CheckoutResult {
  orderId: string;
  subtotal: number;
  discount: number;
  tax: number;
  total: number;
}

interface IPaymentGateway {
  charge(customerId: string, amount: number): Promise<string>;
}

interface IEmailService {
  send(to: string, subject: string, body: string): Promise<void>;
}

// ---------------------------------------------------------------------------
// Stub implementations (satisfy interfaces — not under test)
// ---------------------------------------------------------------------------
class PaymentGateway implements IPaymentGateway {
  async charge(customerId: string, amount: number): Promise<string> {
    console.log(`Charging customer ${customerId} $${amount}`);
    return "order_" + Math.random().toString(36).slice(2);
  }
}

class EmailService implements IEmailService {
  async send(to: string, subject: string, body: string): Promise<void> {
    console.log(`Sending email to ${to}: ${subject}\n${body}`);
  }
}

// ---------------------------------------------------------------------------
// Pure helper — single source of truth for subtotal (fixes DRY violation)
// ---------------------------------------------------------------------------
function calculateSubtotal(items: readonly CartItem[]): number {
  return items.reduce((sum, i) => sum + i.price * i.qty, 0);
}

// ---------------------------------------------------------------------------
// CartValidator — extracted from checkout() (fixes SRP)
// ---------------------------------------------------------------------------
class CartValidator {
  static validate(cart: Cart): void {
    if (!cart.items || cart.items.length === 0) {
      throw new Error("Cart is empty");
    }
  }
}

// ---------------------------------------------------------------------------
// DiscountPolicy — extracted from checkout() (fixes SRP + magic numbers)
// ---------------------------------------------------------------------------
class DiscountPolicy {
  static apply(subtotal: number, itemCount: number): number {
    return itemCount >= BULK_THRESHOLD ? subtotal * BULK_DISCOUNT_RATE : 0;
  }
}

// ---------------------------------------------------------------------------
// CartService — orchestrates only; depends on injected abstractions
// ---------------------------------------------------------------------------
class CartService {
  constructor(
    private readonly gateway: IPaymentGateway,
    private readonly email: IEmailService
  ) {}

  // Fix 1: returns a new Cart instead of mutating the caller's array
  addItem(cart: Cart, item: CartItem): Cart {
    return { ...cart, items: [...cart.items, item] };
  }

  // Fix 3 + 4: uses calculateSubtotal helper and typed Cart parameter
  getTotal(cart: Cart): number {
    const subtotal = calculateSubtotal(cart.items);
    return subtotal + subtotal * TAX_RATE;
  }

  // Fix 5 + 6: delegates validation, discount, payment, and email to collaborators
  async checkout(cart: Cart, customerEmail: string): Promise<CheckoutResult> {
    CartValidator.validate(cart);

    if (!customerEmail.includes("@")) {
      throw new Error("Invalid email address");
    }

    const subtotal = calculateSubtotal(cart.items);
    const discount = DiscountPolicy.apply(subtotal, cart.items.length);
    const discountedSubtotal = subtotal - discount;
    const tax = discountedSubtotal * TAX_RATE;
    const total = discountedSubtotal + tax;

    const orderId = await this.gateway.charge(cart.customerId, total);

    await this.email.send(
      customerEmail,
      "Your order confirmation",
      `Order ${orderId} placed. Total charged: $${total.toFixed(2)}`
    );

    return { orderId, subtotal, discount, tax, total };
  }
}
