const storyPanel = document.querySelector('.story-panel');
const menuLink = document.querySelector('#menu-link');
let storyContent;
let storyError;

function renderStory(story) {
  if (!storyContent) return;
  storyContent.replaceChildren(...story.storyText.split('\n\n').map((paragraph) => {
    const element = document.createElement('p');
    element.textContent = paragraph;
    return element;
  }));
}

function showStory() {
  if (storyPanel) storyPanel.innerHTML = '<div class="section-kicker">Story of a cafe</div><h2 id="story-heading">Pull up a chair.</h2><div id="story-content" class="story-copy" data-testid="story-content" aria-live="polite"><p class="loading-state">Pouring the first cup...</p></div><div id="story-error" class="error-state" data-testid="story-error" hidden>The cafe story is taking a quiet moment. Please try again soon.</div>';
  storyContent = document.querySelector('#story-content');
  storyError = document.querySelector('#story-error');
  void loadStory();
}

function showMenu() {
  if (!storyPanel) return;
  storyPanel.innerHTML = '<div class="section-kicker">Menu management</div><h2 id="menu-heading">Add/Remove Cafe Menu</h2><div class="menu-actions"><button id="add-menu" type="button">Add Menu</button><button id="remove-menu" type="button" disabled>Remove Menu</button></div><div id="menu-region" aria-live="polite"><p class="loading-state">Loading menu...</p></div>';
  document.querySelector('#add-menu')?.addEventListener('click', showAddForm);
  void loadMenu(1);
}

async function loadMenu(page) {
  const region = document.querySelector('#menu-region');
  if (!region) return;
  try {
    const response = await fetch(`/api/menu?page=${page}&pageSize=10`);
    if (!response.ok) throw new Error('Menu request failed');
    renderMenu(await response.json());
  } catch { region.innerHTML = '<div class="error-state">The cafe menu is currently unavailable.</div>'; }
}

function renderMenu(menu) {
  const region = document.querySelector('#menu-region');
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
    checkbox.setAttribute('aria-label', `Select ${item.itemName} ${item.portion}`);
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
  document.querySelector('#previous-page')?.addEventListener('click', () => void loadMenu(menu.page - 1));
  document.querySelector('#next-page')?.addEventListener('click', () => void loadMenu(menu.page + 1));
  document.querySelectorAll('.menu-check').forEach(check => check.addEventListener('change', updateRemoveState));
  document.querySelector('#remove-menu')?.addEventListener('click', () => void removeSelected(menu.page));
}

function updateRemoveState() { const remove = document.querySelector('#remove-menu'); if (remove) remove.disabled = document.querySelectorAll('.menu-check:checked').length === 0; }
async function removeSelected(page) {
  const ids = [...document.querySelectorAll('.menu-check:checked')].map(check => Number(check.value));
  const response = await fetch('/api/menu', { method: 'DELETE', headers: { 'Content-Type': 'application/json' }, body: JSON.stringify({ menuItemIds: ids }) });
  if (response.ok) void loadMenu(page);
}
function showAddForm() {
  const region = document.querySelector('#menu-region');
  if (!region) return;
  region.innerHTML = '<form id="menu-form" class="menu-form"><label for="item-name">Item Name</label><input id="item-name" name="itemName" required><fieldset><legend>Portion</legend><label><input type="radio" name="portion" value="Half" checked> Half</label><label><input type="radio" name="portion" value="Full"> Full</label></fieldset><label for="item-price">Price</label><input id="item-price" name="price" type="number" min="0" step="0.01" required><div class="menu-actions"><button type="submit">Save</button><button id="cancel-menu" type="button">Cancel</button></div><div id="form-errors" class="form-error" role="alert" hidden></div></form>';
  document.querySelector('#cancel-menu')?.addEventListener('click', showMenu);
  document.querySelector('#menu-form')?.addEventListener('submit', event => void saveMenu(event));
}
async function saveMenu(event) {
  event.preventDefault(); const data = new FormData(event.currentTarget);
  const response = await fetch('/api/menu', { method: 'POST', headers: { 'Content-Type': 'application/json' }, body: JSON.stringify({ itemName: data.get('itemName'), portion: data.get('portion'), price: Number(data.get('price')) }) });
  if (response.ok) { showMenu(); return; }
  const errors = document.querySelector('#form-errors'); if (errors) { errors.textContent = response.status === 400 ? 'Please enter a valid item name, portion, and non-negative price.' : 'The menu item could not be saved.'; errors.hidden = false; }
}

async function loadStory() {
  try {
    const response = await fetch('/api/cafe-story/active');
    if (!response.ok) throw new Error(`Story request failed: ${response.status}`);
    renderStory(await response.json());
  } catch {
    if (storyContent) storyContent.hidden = true;
    if (storyError) storyError.hidden = false;
  }
}

storyContent = document.querySelector('#story-content');
storyError = document.querySelector('#story-error');
void loadStory();
menuLink?.addEventListener('click', event => { event.preventDefault(); showMenu(); });
