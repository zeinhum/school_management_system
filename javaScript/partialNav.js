/**
 * @class PartialNav
 * @description
 * Scoped event router for a partial view. Mounted when a partial window
 * loads and destroyed when it is replaced — this is the transient
 * counterpart to the singleton Navigation class.
 *
 * Delegates all click and change events within a root container to
 * handler functions supplied via a map at construction time. Elements
 * declare their intent through data attributes (data-action, data-change)
 * rather than having listeners attached directly — keeping markup clean
 * and handlers centralised.
 *
 * @param {string}   rootSelector - CSS selector for the partial container.
 * @param {Object}   map          - Action-to-handler map.
 *                                  Keys match data-action / data-change values.
 *                                  Values are async functions: (id, event) => void
 *
 * @example
 * const nav = new PartialNav(".partial-container", {
 *   save:   async (id, e) => await saveRecord(id),
 *   delete: async (id, e) => await deleteRecord(id),
 *   filter: async (id, e) => await applyFilter(e.target.value),
 * });
 *
 * // When the partial view is replaced:
 * nav.destroy();
 */
export class PartialNav {
  constructor(rootSelector = ".partial-container", map) {
    this.root = document.querySelector(rootSelector);
    this.funMap = map ?? {};  // guard against undefined map
    if (!this.root) {
      console.warn(`[PartialNav] Root element "${rootSelector}" not found.`);
      return;
    }
    this.handleClick = this.handleClick.bind(this);
    this.handleChange = this.handleChange.bind(this);
    this.init();
  }

  /** Attaches delegated listeners to the root container. */
  init() {
    this.root.addEventListener("click", this.handleClick);
    this.root.addEventListener("change", this.handleChange);
  }

  /**
   * Routes click events to mapped handlers via data-action.
   * Uses closest() so clicks on child elements (icons, spans inside a button)
   * still resolve to the intended action element.
   */
  async handleClick(e) {
    try {
      const actionEl = e.target.closest("[data-action]");
      if (!actionEl || !this.root.contains(actionEl)) return;
      const { action, id } = actionEl.dataset;
      const handler = this.funMap[action];
      if (!action || !handler) return;
      await handler(id, e);
    } catch (err) {
      console.error("[PartialNav] handleClick error:", err);
    }
  }

  /**
   * Routes change events to mapped handlers via data-change.
   * Reads the attribute from the select element itself, not the selected option,
   * keeping the action declaration on the element rather than each option.
   */
  async handleChange(e) {
    try {
      const actionEl = e.target.closest("[data-change]");
      if (!actionEl || !this.root.contains(actionEl)) return;
      const { change, id } = actionEl.dataset;
      const handler = this.funMap[change];
      if (!change || !handler) return;
      await handler(id, e);
    } catch (err) {
      console.error("[PartialNav] handleChange error:", err);
    }
  }

  /**
   * Removes event listeners and releases all references.
   * Must be called when the partial view is replaced to prevent memory leaks.
   * Safe to call multiple times.
   */
  destroy() {
    if (this.root) {
      this.root.removeEventListener("click", this.handleClick);
      this.root.removeEventListener("change", this.handleChange);
    }
    this.root = null;
    this.funMap = null;
    this.handleClick = null;
    this.handleChange = null;
  }
}
