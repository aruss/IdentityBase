/// <reference types="Cypress" />

context('Actions', () => {
  beforeEach(() => {
    cy.visit('http://identitybase.local')
  })

  it('try navigate to auth page', () => {
    cy.get('#menu-secure')
      .click();

    cy.get('#Email')
      .type('alice@localhost');

    cy.get('#Password')
      .type('alice@localhost');

    cy.get('.btn-primary')
      .click();

    cy.get('.btn-primary')
      .click();
  })
})
