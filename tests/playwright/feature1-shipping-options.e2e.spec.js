const path = require('node:path');
const { execFileSync } = require('node:child_process');
const { test, expect } = require('@playwright/test');

// End-to-end browser coverage for the customer-facing Feature 1 checkout flow.
const checkoutId = '17';
const dbHarnessProjectPath = path.resolve(
  __dirname,
  '..',
  'Feature1ShippingOptionDbHarness',
  'Feature1ShippingOptionDbHarness.csproj'
);

function runDbProbe(commandName, selectedCheckoutId) {
  return execFileSync(
    'dotnet',
    [
      'run',
      '--project',
      dbHarnessProjectPath,
      '--',
      commandName,
      selectedCheckoutId,
    ],
    {
    encoding: 'utf8',
    env: {
      ...process.env,
      PRORENTAL_CONNECTION_STRING:
        process.env.PRORENTAL_CONNECTION_STRING ||
        'Host=localhost;Port=5432;Database=pro_rental;Username=postgres;Password=password',
    },
    }
  ).trim();
}

test.beforeEach(() => {
  runDbProbe('reset-checkout', checkoutId);
});

test.afterEach(() => {
  runDbProbe('reset-checkout', checkoutId);
});

test('customer can choose and confirm one shipping preference for seeded checkout 17', async ({ page }) => {
  // The test starts from a known seed state where checkout.option_id is empty.
  expect(runDbProbe('get-selected-option', checkoutId)).toBe('NULL');
  expect(runDbProbe('get-option-count', checkoutId)).toBe('0');

  await page.goto('/');
  await page.getByLabel('Feature 1 Checkout Shipping Options').fill(checkoutId);
  await page.getByRole('button', { name: 'Open' }).click();

  await expect(page.getByRole('heading', { name: 'Choose a shipping preference' })).toBeVisible();
  await expect(page.getByRole('heading', { name: 'Fastest' })).toBeVisible();
  await expect(page.getByRole('heading', { name: 'Cheapest' })).toBeVisible();
  await expect(page.getByRole('heading', { name: 'Greenest' })).toBeVisible();
  await expect(page.getByRole('button', { name: 'Confirm preference' })).toHaveCount(3);
  expect(runDbProbe('get-option-count', checkoutId)).toBe('0');

  await page.getByRole('button', { name: 'Confirm preference' }).first().click();

  await expect(page.getByRole('heading', { name: 'Shipping preference confirmed' })).toBeVisible();

  const details = await page.locator('dd').allTextContents();
  const selectedOptionId = details[1].trim();

  // Confirm that the UI selection also changed the persisted checkout row.
  expect(runDbProbe('get-selected-option', checkoutId)).toBe(selectedOptionId);
  expect(runDbProbe('get-option-count', checkoutId)).toBe('1');
});
