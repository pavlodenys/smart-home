export interface RegistrationFields {
  username: string;
  email: string;
  firstName: string;
  lastName: string;
  password: string;
  confirmPassword: string;
}

export const isRegistrationFormValid = (fields: RegistrationFields): boolean =>
  Boolean(
    fields.username.trim() &&
      fields.email.trim() &&
      fields.firstName.trim() &&
      fields.lastName.trim() &&
      fields.password &&
      fields.password === fields.confirmPassword,
  );
