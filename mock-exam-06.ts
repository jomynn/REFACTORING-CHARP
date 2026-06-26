// =============================================================================
// MOCK REFACTORING EXAM #06 — TypeScript
// Time limit: 60 minutes
// Task: Identify all code smells, then refactor the code below.
// Rules: preserve all existing behaviour, TypeScript strict mode, no new packages.
// =============================================================================
//
// SMELLS TO FIND (fill in before you start coding):
//   1. _______________________________________________
//   2. _______________________________________________
//   3. _______________________________________________
//   4. _______________________________________________
//   5. _______________________________________________
//   6. _______________________________________________
//
// YOUR REFACTORED CODE GOES BELOW THE DASHED LINE AT THE BOTTOM.
// =============================================================================

class PaymentGateway {
  charge(customerId: any, amount: any): string {
    // stub: charge the customer
    console.log(`Charging customer ${customerId} $${amount}`);
    return "order_" + Math.random().toString(36).slice(2);
  }
}

class EmailService {
  send(to: any, subject: any, body: any): void {
    // stub: send an email
    console.log(`Sending email to ${to}: ${subject}`);
  }
}

class CartService {
  addItem(cart: any, item: any): void {
    // SMELL: mutates the caller's array directly
    cart.items.push(item);
  }

  getTotal(items: any): number {
    // SMELL: implicit any, magic number tax rate, inline reduce (DRY violation)
    const subtotal = items.reduce((sum: any, i: any) => sum + i.price * i.qty, 0);
    const tax = subtotal * 0.07;
    return subtotal + tax;
  }

  checkout(cart: any, email: any): any {
    // SMELL: SRP — validation, discount, tax, payment, and email all in one method
    // SMELL: tight coupling — new PaymentGateway() and new EmailService() instantiated here

    if (!cart || !cart.items || cart.items.length === 0) {
      throw new Error("Cart is empty");
    }

    if (!email || !email.includes("@")) {
      throw new Error("Invalid email address");
    }

    // SMELL: magic numbers — 5 and 0.15 hardcoded inline
    let subtotal = cart.items.reduce((sum: any, i: any) => sum + i.price * i.qty, 0);
    let discount = 0;
    if (cart.items.length >= 5) {
      discount = subtotal * 0.15;
    }
    const discountedSubtotal = subtotal - discount;

    // SMELL: magic number 0.07 again (duplicate of getTotal)
    const tax = discountedSubtotal * 0.07;
    const total = discountedSubtotal + tax;

    const gateway = new PaymentGateway();
    const orderId = gateway.charge(cart.customerId, total);

    const mailer = new EmailService();
    mailer.send(
      email,
      "Your order confirmation",
      `Order ${orderId} placed. Total charged: $${total.toFixed(2)}`
    );

    return {
      orderId,
      subtotal,
      discount,
      tax,
      total,
    };
  }
}

// =============================================================================
// YOUR REFACTORED CODE BELOW THIS LINE
// -----------------------------------------------------------------------------
