import assert from "node:assert/strict";
import test from "node:test";

import { isRegistrationFormValid } from "../src/components/register/registration.ts";

const validFields = {
  username: "testuser",
  email: "testuser@example.com",
  firstName: "Test",
  lastName: "User",
  password: "TestUser1!",
  confirmPassword: "TestUser1!",
};

test("registration is enabled when all fields are valid", () => {
  assert.equal(isRegistrationFormValid(validFields), true);
});

test("registration stays disabled when a required field is blank", () => {
  assert.equal(isRegistrationFormValid({ ...validFields, email: " " }), false);
});

test("registration stays disabled when passwords differ", () => {
  assert.equal(
    isRegistrationFormValid({ ...validFields, confirmPassword: "Different1!" }),
    false,
  );
});
