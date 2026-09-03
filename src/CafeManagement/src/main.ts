type CafeStory = {
  storyText: string;
};

type MenuItem = { menuItemId: number; itemName: string; portion: string; price: number };
type MenuPage = { items: MenuItem[]; totalCount: number; page: number; pageSize: number };
type BillOption = { menuItemId: number; itemName: string; portion: 'Half' | 'Full' | 'NA'; price: number };
type BillLine = BillOption & { lineId: number; quantity: number; amount: number };

const storyContent = document.querySelector<HTMLElement>('#story-content');
const storyError = document.querySelector<HTMLElement>('#story-error');
const storyPanel = document.querySelector<HTMLElement>('.story-panel');
const menuLink = document.querySelector<HTMLAnchorElement>('#menu-link');
const calculateBillLink = document.querySelector<HTMLAnchorElement>('#calculate-bill-link');
const contactLink = document.querySelector<HTMLAnchorElement>('[data-testid="nav-contact"]');
let billLines: BillLine[] = [];
let nextBillLineId = 1;
let billOptions: BillOption[] = [];

function money(value: number): string {
  return `Rs ${value.toFixed(2)}`;
}

function escapeHtml(value: string): string {
  return value.replace(/[&<>'"]/g, character => ({ '&': '&amp;', '<': '&lt;', '>': '&gt;', "'": '&#39;', '"': '&quot;' })[character] ?? character);
}

function calculateTotal(): number {
  return billLines.reduce((total, line) => total + Math.round(line.amount * 100), 0) / 100;
}

function showCalculateBill(): void {
  if (!storyPanel) return;
  storyPanel.innerHTML = '<div class="section-kicker">Bill preparation</div><h2 id="bill-heading">Generating Bill</h2><div class="bill-layout"><section class="bill-selection" aria-labelledby="select-item-heading"><h3 id="select-item-heading">Select Item:</h3><form id="bill-form"><label for="bill-item">Select Item:</label><select id="bill-item" required><option value="">Loading menu...</option></select><fieldset><legend>Select Portion:</legend><label><input type="radio" name="bill-portion" value="Half" checked> Half</label><label><input type="radio" name="bill-portion" value="Full"> Full</label></fieldset><label for="bill-quantity">Quantity:</label><select id="bill-quantity"></select><p id="bill-amount" class="bill-amount" aria-live="polite">Amount: Select an item</p><button id="add-to-bill" type="submit">Add To Bill</button><p id="bill-form-error" class="form-error" role="alert" hidden></p></form></section><section class="bill-estimate" aria-labelledby="estimated-bill-heading"><h3 id="estimated-bill-heading">Estimated Bill:</h3><div id="bill-table-region" aria-live="polite"></div><div class="bill-actions"><button id="generate-bill" type="button" disabled>Generate Bill</button><button id="discard-bill" type="button">Discard Bill</button></div></section></div>';
  const quantity = document.querySelector<HTMLSelectElement>('#bill-quantity');
  for (let value = 1; value <= 10; value += 1) quantity?.add(new Option(String(value), String(value)));
  document.querySelector<HTMLFormElement>('#bill-form')?.addEventListener('submit', event => void addBillLine(event));
  document.querySelector<HTMLSelectElement>('#bill-item')?.addEventListener('change', () => { updatePortionAvailability(); updateSelectedAmount(); });
  document.querySelector<HTMLSelectElement>('#bill-quantity')?.addEventListener('change', updateSelectedAmount);
  document.querySelectorAll<HTMLInputElement>('input[name="bill-portion"]').forEach(input => input.addEventListener('change', updateSelectedAmount));
  document.querySelector<HTMLButtonElement>('#generate-bill')?.addEventListener('click', generateBill);
  document.querySelector<HTMLButtonElement>('#discard-bill')?.addEventListener('click', discardBill);
  renderBillTable();
  void loadBillOptions();
}

function showContactUs(): void {
  if (!storyPanel) return;
  storyPanel.innerHTML = '<div class="section-kicker">Contact information</div><h2 id="contact-heading" data-testid="contact-title">Find us At</h2><div class="contact-details" data-testid="contact-content"><section class="contact-section" id="reach-us-section" data-testid="reach-us-section" aria-labelledby="reach-us-heading"><h3 id="reach-us-heading">Reach us at:</h3><address data-testid="cafe-address">&quot;Musafir Cafe&quot;, 7 Hills Road, Pune. 411036</address><p data-testid="cafe-phone">Phone No: +91-9860121455, +91-8485859396</p></section><section class="contact-section" id="connect-us-section" data-testid="connect-us-section" aria-labelledby="connect-us-heading"><h3 id="connect-us-heading">Connect us at:</h3><div class="social-links"><a href="https://www.facebook.com/BeMusafir" target="_blank" rel="noopener noreferrer" data-testid="facebook-link" aria-label="Facebook"><span class="contact-logo contact-logo-facebook" aria-hidden="true">f</span><span>Facebook</span></a><a href="https://www.instagram.com/BeMusafir" target="_blank" rel="noopener noreferrer" data-testid="instagram-link" aria-label="Instagram"><span class="contact-logo contact-logo-instagram" aria-hidden="true">ig</span><span>Instagram</span></a></div></section></div>';
}

async function loadBillOptions(): Promise<void> {
  const select = document.querySelector<HTMLSelectElement>('#bill-item');
  if (!select) return;
  try {
    const response = await fetch('/api/menu/bill-options');
    if (!response.ok) throw new Error('Bill menu request failed');
    billOptions = await response.json() as BillOption[];
    const itemNames = [...new Set(billOptions.map(option => option.itemName))];
    select.replaceChildren(new Option(itemNames.length ? 'Choose an item' : 'No billable items available', ''));
    itemNames.forEach(itemName => select.add(new Option(itemName, itemName)));
    select.disabled = itemNames.length === 0;
  } catch {
    select.replaceChildren(new Option('Menu unavailable', ''));
    select.disabled = true;
    showBillError('The bill menu is currently unavailable.');
  }
}

function selectedPortion(): 'Half' | 'Full' | 'NA' {
  const selectedItem = document.querySelector<HTMLSelectElement>('#bill-item')?.value;
  const itemOptions = billOptions.filter(option => option.itemName === selectedItem);
  if (itemOptions.length > 0 && itemOptions.every(option => option.portion === 'NA')) return 'NA';
  return document.querySelector<HTMLInputElement>('input[name="bill-portion"]:checked')?.value as 'Half' | 'Full' ?? 'Half';
}

function updatePortionAvailability(): void {
  const selectedItem = document.querySelector<HTMLSelectElement>('#bill-item')?.value;
  const itemOptions = billOptions.filter(option => option.itemName === selectedItem);
  const hasPortions = itemOptions.some(option => option.portion === 'Half' || option.portion === 'Full');
  document.querySelectorAll<HTMLInputElement>('input[name="bill-portion"]').forEach(input => { input.disabled = !hasPortions; });
}

function updateSelectedAmount(): void {
  const select = document.querySelector<HTMLSelectElement>('#bill-item');
  const amount = document.querySelector<HTMLElement>('#bill-amount');
  if (!select || !amount) return;
  const option = billOptions.find(item => item.itemName === select.value && item.portion === selectedPortion());
  const quantity = Number(document.querySelector<HTMLSelectElement>('#bill-quantity')?.value ?? 1);
  amount.textContent = option ? `Amount: ${money(option.price * quantity)}` : select.value ? `Amount: ${selectedPortion()} is unavailable for this item` : 'Amount: Select an item';
}

async function addBillLine(event: SubmitEvent): Promise<void> {
  event.preventDefault();
  const item = document.querySelector<HTMLSelectElement>('#bill-item');
  const quantity = document.querySelector<HTMLSelectElement>('#bill-quantity');
  if (!item?.value || !quantity?.value) { showBillError('Select an item and quantity before adding it to the bill.'); return; }
  const button = document.querySelector<HTMLButtonElement>('#add-to-bill');
  if (button) button.disabled = true;
  try {
    const selected = billOptions.find(option => option.itemName === item.value && option.portion === selectedPortion());
    if (!selected) throw new Error('Selected portion is unavailable');
    const response = await calculateLine(selected.menuItemId, selectedPortion(), Number(quantity.value));
    if (!response.ok) throw new Error('Bill calculation failed');
    const line = await response.json() as Omit<BillLine, 'lineId'>;
    billLines.push({ ...line, lineId: nextBillLineId++ });
    renderBillTable();
    item.value = '';
    updateSelectedAmount();
  } catch {
    showBillError('The selected item could not be added to the bill.');
  } finally {
    if (button) button.disabled = false;
  }
}

function calculateLine(menuItemId: number, portion: string, quantity: number): Promise<Response> {
  return fetch('/api/bill/calculate', { method: 'POST', headers: { 'Content-Type': 'application/json' }, body: JSON.stringify({ menuItemId, portion, quantity }) });
}

function showBillError(message: string): void {
  const error = document.querySelector<HTMLElement>('#bill-form-error');
  if (error) { error.textContent = message; error.hidden = false; }
}

function renderBillTable(): void {
  const region = document.querySelector<HTMLElement>('#bill-table-region');
  if (!region) return;
  if (billLines.length === 0) {
    region.innerHTML = `<p class="empty-state">No items added yet. Total: ${money(0)}</p>`;
  } else {
    const table = document.createElement('table');
    table.className = 'bill-table';
    table.dataset.testid = 'bill-table';
    table.innerHTML = '<thead><tr><th scope="col">Item</th><th scope="col">Portion</th><th scope="col">Quantity</th><th scope="col">Price</th><th scope="col">Amount</th><th scope="col">Edit</th><th scope="col">Remove</th></tr></thead>';
    const body = document.createElement('tbody');
    billLines.forEach(line => {
      const row = document.createElement('tr');
      row.dataset.lineId = String(line.lineId);
      [line.itemName, line.portion, String(line.quantity), money(line.price), money(line.amount)].forEach(value => { const cell = document.createElement('td'); cell.textContent = value; row.append(cell); });
      const edit = document.createElement('button'); edit.type = 'button'; edit.textContent = '✎'; edit.ariaLabel = `Edit quantity for ${line.itemName}`; edit.addEventListener('click', () => void editBillLine(line.lineId));
      const remove = document.createElement('button'); remove.type = 'button'; remove.textContent = '×'; remove.ariaLabel = `Remove ${line.itemName}`; remove.addEventListener('click', () => removeBillLine(line.lineId));
      const editCell = document.createElement('td'); editCell.append(edit); const removeCell = document.createElement('td'); removeCell.append(remove); row.append(editCell, removeCell); body.append(row);
    });
    table.append(body);
    region.replaceChildren(table, Object.assign(document.createElement('p'), { className: 'bill-total', textContent: `Total: ${money(calculateTotal())}` }));
  }
  const generate = document.querySelector<HTMLButtonElement>('#generate-bill');
  if (generate) generate.disabled = billLines.length === 0;
}

async function editBillLine(lineId: number): Promise<void> {
  const line = billLines.find(item => item.lineId === lineId);
  if (!line) return;
  const value = window.prompt(`Quantity for ${line.itemName} (1-10):`, String(line.quantity));
  if (value === null) return;
  const quantity = Number(value);
  if (!Number.isInteger(quantity) || quantity < 1 || quantity > 10) { showBillError('Quantity must be a whole number between 1 and 10.'); return; }
  try {
    const response = await calculateLine(line.menuItemId, line.portion, quantity);
    if (!response.ok) throw new Error('Bill recalculation failed');
    const updated = await response.json() as Omit<BillLine, 'lineId'>;
    billLines = billLines.map(item => item.lineId === lineId ? { ...updated, lineId } : item);
    renderBillTable();
  } catch { showBillError('The quantity could not be updated.'); }
}

function removeBillLine(lineId: number): void {
  billLines = billLines.filter(line => line.lineId !== lineId);
  renderBillTable();
}

function generateBill(): void {
  if (billLines.length === 0 || !window.confirm('Generate this bill?')) return;
  showFinalBill();
}

function discardBill(): void {
  if (!window.confirm('Discard this bill?')) return;
  billLines = [];
  showCalculateBill();
}

function showFinalBill(): void {
  if (!storyPanel) return;
  storyPanel.innerHTML = `<div class="section-kicker">Final bill</div><h2 id="final-bill-heading">Musafir Cafe</h2><section class="final-bill" data-testid="final-bill" aria-labelledby="final-bill-heading"><table class="bill-table"><thead><tr><th scope="col">Item</th><th scope="col">Portion</th><th scope="col">Quantity</th><th scope="col">Price</th><th scope="col">Amount</th></tr></thead><tbody>${billLines.map(line => `<tr><td>${escapeHtml(line.itemName)}</td><td>${escapeHtml(line.portion)}</td><td>${line.quantity}</td><td>${money(line.price)}</td><td>${money(line.amount)}</td></tr>`).join('')}</tbody></table><p class="bill-total">Total: ${money(calculateTotal())}</p><div class="bill-actions"><button id="print-bill" type="button">Print</button><button id="new-bill" type="button">Generate New Bill</button></div></section>`;
  document.querySelector<HTMLButtonElement>('#print-bill')?.addEventListener('click', () => window.print());
  document.querySelector<HTMLButtonElement>('#new-bill')?.addEventListener('click', () => { billLines = []; showCalculateBill(); });
}

function showMenu(): void {
  if (!storyPanel) return;
  storyPanel.innerHTML = '<div class="section-kicker">Menu management</div><h2 id="menu-heading">Add/Remove Cafe Menu</h2><div class="menu-actions"><button id="add-menu" type="button">Add Menu</button><button id="remove-menu" type="button" disabled>Remove Menu</button></div><div id="menu-region" aria-live="polite"><p class="loading-state">Loading menu...</p></div>';
  document.querySelector<HTMLButtonElement>('#add-menu')?.addEventListener('click', showAddForm);
  void loadMenu(1);
}

async function loadMenu(page: number): Promise<void> {
  const region = document.querySelector<HTMLElement>('#menu-region');
  if (!region) return;
  try {
    const response = await fetch(`/api/menu?page=${page}&pageSize=10`);
    if (!response.ok) throw new Error('Menu request failed');
    renderMenu(await response.json() as MenuPage);
  } catch {
    region.innerHTML = '<div class="error-state">The cafe menu is currently unavailable.</div>';
  }
}

function renderMenu(menu: MenuPage): void {
  const region = document.querySelector<HTMLElement>('#menu-region');
  if (!region) return;
  const totalPages = Math.max(1, Math.ceil(menu.totalCount / menu.pageSize));
  const table = document.createElement('table');
  table.className = 'menu-table';
  table.dataset.testid = 'menu-table';
  table.innerHTML = '<thead><tr><th scope="col">Select</th><th scope="col">Name of Item</th><th scope="col">Portion</th><th scope="col">Price</th></tr></thead>';
  const body = document.createElement('tbody');
  menu.items.forEach(item => {
    const row = document.createElement('tr');
    const selectionCell = document.createElement('td');
    const checkbox = document.createElement('input');
    checkbox.className = 'menu-check';
    checkbox.type = 'checkbox';
    checkbox.value = String(item.menuItemId);
    checkbox.ariaLabel = `Select ${item.itemName} ${item.portion}`;
    selectionCell.append(checkbox);
    row.append(selectionCell);
    [item.itemName, item.portion, item.price.toFixed(2)].forEach(value => {
      const cell = document.createElement('td');
      cell.textContent = value;
      row.append(cell);
    });
    body.append(row);
  });
  table.append(body);
  const pagination = document.createElement('div');
  pagination.className = 'pagination';
  pagination.innerHTML = `<button id="previous-page" type="button" ${menu.page <= 1 ? 'disabled' : ''}>Previous</button><span>Page ${menu.page} of ${totalPages}</span><button id="next-page" type="button" ${menu.page >= totalPages ? 'disabled' : ''}>Next</button>`;
  region.replaceChildren(table, pagination);
  document.querySelector<HTMLButtonElement>('#previous-page')?.addEventListener('click', () => void loadMenu(menu.page - 1));
  document.querySelector<HTMLButtonElement>('#next-page')?.addEventListener('click', () => void loadMenu(menu.page + 1));
  document.querySelectorAll<HTMLInputElement>('.menu-check').forEach(check => check.addEventListener('change', updateRemoveState));
  document.querySelector<HTMLButtonElement>('#remove-menu')?.addEventListener('click', () => void removeSelected(menu.page));
}

function updateRemoveState(): void {
  const remove = document.querySelector<HTMLButtonElement>('#remove-menu');
  if (remove) remove.disabled = document.querySelectorAll<HTMLInputElement>('.menu-check:checked').length === 0;
}

async function removeSelected(page: number): Promise<void> {
  const ids = [...document.querySelectorAll<HTMLInputElement>('.menu-check:checked')].map(check => Number(check.value));
  const response = await fetch('/api/menu', { method: 'DELETE', headers: { 'Content-Type': 'application/json' }, body: JSON.stringify({ menuItemIds: ids }) });
  if (response.ok) void loadMenu(page);
}

function showAddForm(): void {
  const region = document.querySelector<HTMLElement>('#menu-region');
  if (!region) return;
  region.innerHTML = '<form id="menu-form" class="menu-form"><label for="item-name">Item Name</label><input id="item-name" name="itemName" required><fieldset><legend>Portion</legend><label><input type="radio" name="portion" value="Half" checked> Half</label><label><input type="radio" name="portion" value="Full"> Full</label></fieldset><label for="item-price">Price</label><input id="item-price" name="price" type="number" min="0" step="0.01" required><div class="menu-actions"><button type="submit">Save</button><button id="cancel-menu" type="button">Cancel</button></div><div id="form-errors" class="form-error" role="alert" hidden></div></form>';
  document.querySelector<HTMLButtonElement>('#cancel-menu')?.addEventListener('click', showMenu);
  document.querySelector<HTMLFormElement>('#menu-form')?.addEventListener('submit', event => void saveMenu(event));
}

async function saveMenu(event: SubmitEvent): Promise<void> {
  event.preventDefault();
  const form = event.currentTarget as HTMLFormElement;
  const data = new FormData(form);
  const response = await fetch('/api/menu', { method: 'POST', headers: { 'Content-Type': 'application/json' }, body: JSON.stringify({ itemName: data.get('itemName'), portion: data.get('portion'), price: Number(data.get('price')) }) });
  if (response.ok) { showMenu(); return; }
  const errors = document.querySelector<HTMLElement>('#form-errors');
  if (errors) { errors.textContent = response.status === 400 ? 'Please enter a valid item name, portion, and non-negative price.' : 'The menu item could not be saved.'; errors.hidden = false; }
}

function renderStory(story: CafeStory): void {
  if (!storyContent) return;
  storyContent.replaceChildren(
    ...story.storyText.split('\n\n').map((paragraph) => {
      const element = document.createElement('p');
      element.textContent = paragraph;
      return element;
    })
  );
}

async function loadStory(): Promise<void> {
  try {
    const response = await fetch('/api/cafe-story/active');
    if (!response.ok) throw new Error(`Story request failed: ${response.status}`);
    renderStory(await response.json() as CafeStory);
  } catch {
    if (storyContent) storyContent.hidden = true;
    if (storyError) storyError.hidden = false;
  }
}

void loadStory();
menuLink?.addEventListener('click', event => { event.preventDefault(); showMenu(); });
calculateBillLink?.addEventListener('click', event => { event.preventDefault(); showCalculateBill(); });
contactLink?.addEventListener('click', event => { event.preventDefault(); showContactUs(); });
